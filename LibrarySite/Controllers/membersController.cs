using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using LibrarySite.Models;
using PagedList;

namespace LibrarySite.Controllers
{
    [HandleError]
    public class membersController : Controller
    {
        private libraryEntities db = new libraryEntities();
        private Utilities util = new Utilities();
        public bool thisIsUnitTest { get; set; }
        public libraryEntities DB { get { return db; } }

        public membersController()
        {
        }

        public ActionResult Autocomplete(string term)
        {
            var model =
                db.members
                   .Where(r => r.lastname.StartsWith(term))
                   .Take(10)
                   .Select(r => new
                   {
                       label = r.lastname
                   }).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        // GET: members
        [Authorize(Roles = "Librarian")]
        public ActionResult Index(string searchTerm = null, int page = 1, int pageSize = 10)
        {                 //db.members.Include(m => m.adult).Include(m => m.juvenile)
            var members = db.members.Include(m => m.adult).Include(m => m.juvenile)
                          .OrderBy(m => m.lastname).ThenBy(m => m.firstname).ThenBy(m => m.member_no)
                          .Where(r => searchTerm == null || r.lastname.StartsWith(searchTerm))
                          //.Take(10)
                          //.ToList();
                          .ToPagedList(page, pageSize);

            if (!thisIsUnitTest)
            { 
                Session["searchTerm"] = searchTerm;
                Session["page"] = page;
                Session["pageSize"] = pageSize;
            }  

            if (Request.IsAjaxRequest())
            {
                return PartialView("_members", members);
            }
           return View(members);
        }

        // GET: members/GetMemberInfo
        [Authorize(Roles = "Librarian, Member")]
        public ActionResult GetMemberInfo(short? member_no)
        {
            ViewBag.members = util.MembersList;
            if (member_no == null)
            {
                return View();
            }
            var member = db.members.Include(m => m.adult).Include(m => m.juvenile).Include(m => m.loans)
                          .Where(r => r.member_no == member_no).ToList();
            if (member == null)
            {
                return HttpNotFound();
            }
            // if juvenile member, check if 18 or older then convert to adult
            ViewBag.JuvenileConverted = false;
            if (member[0].juvenile != null && member[0].juvenile.birth_date.AddYears(18).Date <= DateTime.Now.Date)
            {
                // Save member before update in case juvenile update does not work
                if (!ConvertJuvenileToAdult(member[0], member[0].juvenile)) {
                    //Retrieve member data to current values since values were updated
                    using (var context = new libraryEntities())
                    {
                        var currentMember = context.members.Include(m => m.adult).Include(m => m.juvenile).Include(m => m.juvenile.adult)
                            .Include(m => m.juvenile.adult.member).Include(m => m.loans)
                          .Where(r => r.member_no == member_no).ToList();
                        member = currentMember;
                    }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_memberLoans", member);
            }
            
            return View(member);
        }

        // POST: members/CreateAdult
        [Authorize(Roles = "Librarian, Member")]
        [HttpPost, ActionName("GetMemberInfo")]
        [ValidateAntiForgeryToken]
        public ActionResult GetMemberInfoUpdate(short member_no)
        {
            // Renew Adult Members Expiration Date
            bool updateModel = false;
            ViewBag.members = util.MembersList;
            ViewBag.JuvenileConverted = false;
            var memberToUpdate = db.members.Include(m => m.adult).Include(m => m.juvenile).Include(m => m.loans)
                .Where(r => r.member_no == member_no).ToList();
            DateTime oldExprDate = memberToUpdate[0].adult.expr_date;

            if (ModelState.IsValid)
            {
                updateModel = TryUpdateModel(memberToUpdate[0], "", new string[] {"expr_date"});
                memberToUpdate[0].adult.expr_date = DefaultValues.NewExpirationDate;

                if (updateModel)
                {
                    try
                    {
                        db.SaveChanges();
                        TempData["UserMessageSuccess"] = "Renew Adult Member's Expiration Date Completed.";
                        return View(memberToUpdate);
                    }
                    catch (DbEntityValidationException e)
                    {                     
                        GetValidationErrors(e, "GetMemberInfo");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("","Exception: " + ex.Message);
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
            }

            memberToUpdate[0].adult.expr_date = oldExprDate;
            TempData["UserMessageError"] = "Renew Adult Member's Expiration Date Failed.";
            return View(memberToUpdate);          
        }

        private bool ConvertJuvenileToAdult(member member, juvenile juvenile)
        {
            StringBuilder errorMsg = new StringBuilder();

            using (var context = new libraryEntities())
            {
                using (var dbContextTranaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        member.adult = new Models.adult();
                        UpdateAdultData(juvenile.adult, member);
                        db.juveniles.Remove(juvenile);

                        if (ModelState.IsValid)
                        {
                            db.adults.Add(member.adult);
                            db.SaveChanges();
                            dbContextTranaction.Commit();
                            TempData["JuvenileConvertMessage"] = "Juvenile member converted to Adult member Completed. Converted due to juvenile being 18 years or older.";
                            return true;
                        }

                    }
                    catch (DbEntityValidationException e)
                    {
                        string memberID = "Member" + member.member_no;
                        GetValidationErrors(e, memberID);
                        foreach(var error in ModelState[memberID + "ValidationErrors"].Errors)
                        {
                            errorMsg.AppendLine(error.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg.AppendLine("Exception: " + ex.Message);
                    }

                    errorMsg.AppendLine("Juvenile member converted to Adult member Failed. Try again, and if the problem persists, see your system administrator.");
                    TempData["JuvenileConvertMessage"] = errorMsg.ToString();
                    dbContextTranaction.Rollback();
                    return false;
                }
            }
        }

        // GET: members/Details/5
        [Authorize(Roles = "Librarian")]
        public ActionResult Details(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            member member = db.members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }

            return View(member);
        }

        // GET: members/CreateAdult
        [Authorize(Roles = "Librarian, Member")]
        public ActionResult CreateAdult()
        {
            return View();
        }

        // GET: members/CreateJuvenile
        [Authorize(Roles = "Librarian, Member")]
        public ActionResult CreateJuvenile()
        {
            ViewBag.AdultMembers = util.AdultMembersList;
            return View();
        }

        // POST: members/CreateAdult
        [Authorize(Roles = "Librarian, Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdult([Bind(Include="lastname,firstname,middleinitial,adult.street,adult.city,adult.state,adult.zip,adult.phone_no", Exclude="member_no, adult.member_no")] 
                                         member member, adult adult)
        {
            try
            {    
                member.adult = new Models.adult();
                UpdateAdultData(adult, member, false);

                if (ModelState.IsValid)
                {
                    db.members.Add(member);
                    db.SaveChanges();
                    TempData["UserMessageSuccess"] = "Create Adult Completed.";
                    return RedirectToAction("Index","Home");
                }
            }
            catch (DbEntityValidationException e)
            {
                GetValidationErrors(e, "CreateAdult");
            }
            catch 
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            ViewBag.AdultMembers = util.AdultMembersList;
            TempData["UserMessageError"] = "Create Adult Failed.";
            return View(member);
        }

        // POST: members/CreateJuvenile
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Librarian, Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateJuvenile([Bind(Include="lastname,firstname,middleinitial,juvenile.adult_member_no,juvenile.birth_date")] member member, juvenile juvenile)
        {
            try
            {
                member.juvenile = new Models.juvenile();
                UpdateJuvenileData(juvenile, member);

                if (ModelState.IsValid)
                {
                    db.members.Add(member);
                    db.SaveChanges();
                    TempData["UserMessageSuccess"] = "Create Juvenile Completed.";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (DbEntityValidationException e)
            {
                GetValidationErrors(e, "CreateJuvenile");
            }
            catch 
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            TempData["UserMessageError"] = "Create Juvenile Failed.";
            return View(member);
        }

        // GET: members/EditAdult/5
        [Authorize(Roles = "Librarian")]
        public ActionResult EditAdult(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            member member = db.members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }

            return View(member);
        }

        // GET: members/EditJuvenile/5
        [Authorize(Roles = "Librarian")]
        public ActionResult EditJuvenile(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            member member = db.members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }

            ViewBag.AdultMembers = util.AdultMembersList;
            return View(member);
        }

        // POST: members/EditAdult/5
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAdult([Bind(Include = "lastname,firstname,middleinitial,adult.street,adult.city,adult.state,adult.zip,adult.phone_no, member_no")] member member, adult adult)
        {
            bool updateModel = false;
            if (ModelState.IsValid)
            {
                var memberToUpdate = db.members
                    .Include(i => i.adult)
                    .Where(i => i.member_no == member.member_no)
                    .Single();
                updateModel = TryUpdateModel(memberToUpdate, "", new string[] {"lastname", "firstname", "middleinitial", "photograph",
                            "street", "city", "state", "zip", "phone_no", "expr_date"});
                UpdateAdultData(adult, memberToUpdate);

                if (updateModel)
                {
                    try
                    {
                        db.SaveChanges();
                        TempData["UserMessageSuccess"] = "Edit Adult Completed.";
                        return RedirectToAction("Index", new { searchTerm = Session["searchTerm"], page = Session["page"], pageSize = Session["pageSize"] });
                    }                    
                    catch 
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                 }                
            }

            TempData["UserMessageError"] = "Edit Adult Failed.";
            return View(member);
        }

        // POST: members/EditJuvenile/5
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditJuvenile([Bind(Include = "lastname,firstname,middleinitial,juvenile.adult_member_no,juvenile.birth_date, member_no")] member member, juvenile juvenile)
        {
            bool updateModel = false;
            if (ModelState.IsValid)
            {
                var memberToUpdate = db.members
                    .Include(i => i.juvenile)
                    .Where(i => i.member_no == member.member_no)
                    .Single();
                updateModel = TryUpdateModel(memberToUpdate, "", new string[] {"lastname", "firstname", "middleinitial", "photograph",
                            "adult_member_no", "birth_date"});
                UpdateJuvenileData(juvenile, memberToUpdate);

                if (updateModel)
                {
                    try
                    {
                        db.SaveChanges();
                        TempData["UserMessageSuccess"] = "Edit Juvenile Completed.";
                        return RedirectToAction("Index", new { searchTerm = Session["searchTerm"], page = Session["page"], pageSize = Session["pageSize"] });
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
            }

            ViewBag.AdultMembers = util.AdultMembersList;
            TempData["UserMessageError"] = "Edit Juvenile Failed.";
            return View(member);
        }

        private void UpdateAdultData(adult adult, member memberToUpdate, bool updateExprDate = true)
        {
            memberToUpdate.adult.street = adult.street;
            memberToUpdate.adult.city = adult.city;
            memberToUpdate.adult.state = adult.state;
            memberToUpdate.adult.zip = adult.zip;
            if (adult.phone_no != null) memberToUpdate.adult.phone_no = adult.phone_no;
            if (updateExprDate) memberToUpdate.adult.expr_date = adult.expr_date;
        }

        private void UpdateJuvenileData(juvenile juvenile, member memberToUpdate)
        {
            memberToUpdate.juvenile.adult_member_no = juvenile.adult_member_no;
            memberToUpdate.juvenile.birth_date = juvenile.birth_date;
        }

        // GET: members/Delete/5
        [Authorize(Roles = "Librarian")]
        public ActionResult Delete(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            member member = db.members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }

            return View(member);
        }

        // POST: members/Delete/5
        [Authorize(Roles = "Librarian")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(short id)
        {
            member member = db.members.Find(id);
            // if adult member then delete juvenile member records since juvenile member records are not deleted
            if (member.adult != null)
            {
                DeleteJuvenileMemberRecords(id);
            }

            try
            {
                db.members.Remove(member);
                db.SaveChanges();
                TempData["UserMessageSuccess"] = "Delete Member Completed.";
                return RedirectToAction("Index", new { searchTerm = Session["searchTerm"], page = Session["page"], pageSize = Session["pageSize"] });
            }
            catch
            {
                ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
            }
            TempData["UserMessageError"] = "Delete Member Failed.";
            return View(member);
        }

        private void DeleteJuvenileMemberRecords(short id)
        {
            var jMembers = db.members.Include(m => m.juvenile)
                  .Where(m => m.juvenile.adult_member_no == id && m.member_no == m.juvenile.member_no);  
            // if juvenile member records exist with adult member number equal to id 
            //   then delete juvenile member records since cascade delete does not delete the juvenile member records
            if (jMembers != null)
            {
                db.members.RemoveRange(jMembers);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        protected void GetValidationErrors(DbEntityValidationException e, string ActionName)
        {
            string key = ActionName + "ValidationErrors";
            foreach (var eve in e.EntityValidationErrors)
            {
                String s = String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                ModelState.AddModelError(key, s);
                foreach (var ve in eve.ValidationErrors)
                {
                    s = String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    ModelState.AddModelError(key, s);
                }
            }
        }
    }
}
