using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LibrarySite.Models;


namespace LibrarySite.Controllers
{
    [HandleError]
    public class LoansController : Controller
    {
        private libraryEntities db = new libraryEntities();
        private Utilities util = new Utilities();

        // GET: loans
        [Authorize(Roles = "Librarian")]
        public ActionResult Index()
        {
            var loans = db.loans.Include(l => l.copy).Include(l => l.member).Include(l => l.title)
                        .OrderByDescending(m => m.out_date);
            return View(loans.ToList());
        }

        public SelectList LoanableISBNList
        {
            get
            {
                var isbnList = db.items.OrderBy(m => m.isbn).Where(m => m.loanable == "Y").Select(m => new SelectListItem()
                {
                    Text = m.isbn.ToString(),
                    Value = m.isbn.ToString(),
                });
                return new SelectList(isbnList, "Value", "Text");
            }
        }

        public SelectList GetTitle_NoListByISBNCopy_No(int isbn, short copy_no)
        {
            var title_noList = db.copies.Include(m => m.title)
                .Where(m => m.isbn == isbn && m.copy_no == copy_no).Select(m => new SelectListItem()
                {
                    Text = m.title.title1.ToString(),
                    Value = m.title_no.ToString(),
                });
            return new SelectList(title_noList, "Value", "Text");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetCopy_NoListByISBN(int isbn)
        {
            var copy_noList = db.copies.Where(m => m.isbn == isbn).Select(m => new SelectListItem()
            {
                Text = m.copy_no.ToString(),
                Value = m.copy_no.ToString(),
            });
            return Json(copy_noList, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetLoanableISBNList()
        {
            return Json(LoanableISBNList, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetLoanInfo(int isbn, short copy_no)
        {
            var loanCopyInfo = new List<object>();
            var prevLoanInfo = db.loans.Include(m => m.title).Include(m => m.member)
                              .Where(m => m.isbn == isbn && m.copy_no == copy_no).ToList();

            if (prevLoanInfo.Count == 1)
            {
                loanCopyInfo.Add(new { title = prevLoanInfo[0].title.title1, author = prevLoanInfo[0].title.author,
                                       lastname = prevLoanInfo[0].member.lastname, firstname = prevLoanInfo[0].member.firstname,
                                       title_no = prevLoanInfo[0].title_no.ToString(), member_no = prevLoanInfo[0].member_no.ToString(),
                                       due_date = prevLoanInfo[0].due_date.ToString()});
            }
            else
            {
                var copyInfo = db.copies.Include(m => m.title).Where(m => m.isbn == isbn && m.copy_no == copy_no).ToList();
                if (copyInfo.Count == 1)
                {
                    loanCopyInfo.Add(new { title = copyInfo[0].title.title1, author = copyInfo[0].title.author,
                        lastname = "", title_no = copyInfo[0].title_no.ToString()});
                }
            }

            TempData["prevLoanInfo"] = prevLoanInfo;
            return Json(loanCopyInfo, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult ReachedCheckOutItemExprDateLimit(short member_no)
        {
            string reachedLimitMsg = "";
            int numCheckedOut = db.loans.Where(m => m.member_no == member_no).Count();
            if (numCheckedOut >= DefaultValues.MaxCheckOutItemLimit)
            {
                reachedLimitMsg = "Maximum limit (" + DefaultValues.MaxCheckOutItemLimit + ") of checked out books reached.  ";
            }

            var member = db.members.Include(m => m.adult).Include(m => m.juvenile).Where(m => m.member_no == member_no).Single();
            if (member.adult != null && member.adult.expr_date.AddYears(1).Date <= DateTime.Now.Date)
            {
                reachedLimitMsg += "Member expiration date needs to be renewed.";
            }

            if (member.juvenile != null && member.juvenile.adult.expr_date.AddYears(1).Date <= DateTime.Now.Date)
            {
                reachedLimitMsg += "Adult Member expiration date needs to be renewed.";
            }

            return Json(reachedLimitMsg, JsonRequestBehavior.AllowGet);
        }

        // GET: loans/Details/5
        public ActionResult Details(int? isbn, short? copy_no)
        {
            if (isbn == null || copy_no == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            loan loan = db.loans.Find(isbn, copy_no);
            if (loan == null)
            {
                return HttpNotFound();
            }
            return View(loan);
        }

        // GET: loans/CheckIn
        [Authorize(Roles="Librarian")]
        public ActionResult CheckIn(string Message = "")
        {
            ViewBag.isbn = LoanableISBNList;
            return View();
        }

        // POST: loans/CheckIn
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckIn([Bind(Include = "isbn,copy_no")] loan loan)
        {
            bool updateModel = false;
            List<loan> prevLoanInfo = TempData["prevLoanInfo"] as List<loan>;

            if (ModelState.IsValid && prevLoanInfo.Count ==  1)
            {
                DoCheckInTransactions(prevLoanInfo[0], ref updateModel);
                if (updateModel)
                {
                    TempData["UserMessageSuccess"] = "Check In Completed.";               
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["UserMessageError"] = "Check Out Failed.";
            ViewBag.isbn = LoanableISBNList;
            return View();
        }

        // GET: loans/CheckOut
        [Authorize(Roles = "Librarian")]
        public ActionResult CheckOut()
        {
            ViewBag.returnUrl = Request.UrlReferrer;
            ViewBag.members = util.MembersList;
            return View(new loan());
        }

        // POST: loans/CheckOut
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut([Bind(Include = "member_no,isbn,copy_no,title_no,out_date,due_date")] loan loan, string returnUrl)
        {
            bool updateModel = false;
            List<loan> prevLoanInfo = TempData["prevLoanInfo"] as List<loan>;

            if (ModelState.IsValid)
            {
                //Check if item is already on loan, if so, check in first.
                if (prevLoanInfo.Count == 1)
                {
                    DoCheckInTransactions(prevLoanInfo[0], ref updateModel);
                }

                if ((prevLoanInfo.Count == 1 && updateModel) || prevLoanInfo.Count == 0)
                {
                    // CheckOut, create loan with isbn and copy_no and update Copy with isbn and copy_no
                    db.loans.Add(loan);
                    // Update copy on_load value
                    var copyToUpdate = db.copies.Where(m => m.isbn == loan.isbn && m.copy_no == loan.copy_no).Single();
                    bool updateModel2 = TryUpdateModel(copyToUpdate, "", new string[] { "isbn", "copy_no", "title_no", "on_loan" });
                    copyToUpdate.on_loan = "Y";
                    if (updateModel2)
                    {
                        try
                        {
                            db.SaveChanges();
                            string Msg = "";
                            if (updateModel) Msg = "Check In Completed. ";
                            if (updateModel2) Msg += "Check Out Completed.";
                            TempData["UserMessageSuccess"] = Msg;
                            return Redirect(returnUrl);
                        }
                        catch 
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                        }
                    }
                }
            }

            TempData["UserMessageError"] = "Check Out Failed.";
            ViewBag.returnUrl = returnUrl;
            ViewBag.members = util.MembersList;
            return View(new loan());
        }

        private void DoCheckInTransactions(loan prevLoan, ref bool updateModel)
        {
            // Insert loan info into loanhist
            loanhist loanhist = new loanhist();
            loanhist.isbn = prevLoan.isbn;
            loanhist.copy_no = prevLoan.copy_no;
            loanhist.title_no = prevLoan.title_no;
            loanhist.member_no = prevLoan.member_no;
            loanhist.out_date = prevLoan.out_date;
            loanhist.due_date = prevLoan.due_date;
            loanhist.in_date = DateTime.Now;
            db.loanhists.Add(loanhist);
            // Delete loan
            db.loans.Attach(prevLoan);
            db.loans.Remove(prevLoan);
            // Update copy
            var copyToUpdate = db.copies.Where(m => m.isbn == prevLoan.isbn && m.copy_no == prevLoan.copy_no).Single();
            updateModel = TryUpdateModel(copyToUpdate, "", new string[] { "isbn", "copy_no", "title_no", "on_loan" });
            copyToUpdate.on_loan = "N";

            if (updateModel)
            {
                try
                {
                    db.SaveChanges();
                    return;
                }
                catch 
                {
                    updateModel = false;
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
        }

        // GET: loans/Create
        //    Create Loan is the same as Check Out
        [Authorize(Roles = "Librarian")]
        public ActionResult Create()
        {
            ViewBag.members = util.MembersList; 
            return View(new loan());
        }

        // POST: loans/Create
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "isbn,copy_no,title_no,member_no,out_date,due_date")] loan loan)
        {
            if (ModelState.IsValid)
            {
                db.loans.Add(loan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.isbn = new SelectList(db.copies, "isbn", "on_loan", loan.isbn);
            ViewBag.member_no = new SelectList(db.members, "member_no", "lastname", loan.member_no);
            ViewBag.title_no = new SelectList(db.titles, "title_no", "title1", loan.title_no);
            return View(loan);
        }

        // GET: loans/Edit/5
        [Authorize(Roles = "Librarian")]
        public ActionResult Edit(int? isbn, short? copy_no)
        {
            if (isbn == null || copy_no == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            loan loan = db.loans.Find(isbn, copy_no);
            if (loan == null)
            {
                return HttpNotFound();
            }
            ViewBag.member_no = util.MembersList;
            ViewBag.title_no = GetTitle_NoListByISBNCopy_No(isbn.Value, copy_no.Value);
            return View(loan);
        }

        // POST: loans/Edit/5
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "isbn,copy_no,title_no,member_no,out_date,due_date")] loan loan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(loan).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["UserMessageSuccess"] = "Save changes completed.";
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            TempData["UserMessageError"] = "Save Changes Failed.";
            ViewBag.member_no = util.MembersList;
            ViewBag.title_no = GetTitle_NoListByISBNCopy_No(loan.isbn, loan.copy_no);
            return View(loan);
        }

        // GET: loans/Delete/5
        // This is the process of deleting a loan that is Check In of Item
        [Authorize(Roles = "Librarian")]
        public ActionResult Delete(int? isbn, short? copy_no)
        {
            if (isbn == null || copy_no == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            loan loan = db.loans.Find(isbn, copy_no);
            if (loan == null)
            {
                return HttpNotFound();
            }
            return View(loan);
        }

        // POST: loans/Delete/5
        [Authorize(Roles = "Librarian")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int isbn, short copy_no)
        {
            bool updateModel = false;
            loan loan = db.loans.Find(isbn, copy_no);
            DoCheckInTransactions(loan, ref updateModel);

            if (updateModel)
            {
                TempData["UserMessageSuccess"] = "Delete(Check In) Completed.";
                return RedirectToAction("Index");
            }

            TempData["UserMessageError"] = "Delete(Check In) Failed.";
            return View(loan);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
