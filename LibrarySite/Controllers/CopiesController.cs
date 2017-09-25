using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LibrarySite.Models;

namespace LibrarySite.Controllers
{
    [HandleError]
    public class CopiesController : Controller
    {
        private libraryEntities db = new libraryEntities();

        // GET: Copies
        public ActionResult Index()
        {
            var copies = db.copies.Include(c => c.item).Include(c => c.title).Include(c => c.loan);
            return View(copies.ToList());
        }

        // GET: Copies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            copy copy = db.copies.Find(id);
            if (copy == null)
            {
                return HttpNotFound();
            }
            return View(copy);
        }

       
        // GET: Copies/Create
        [Authorize(Roles = "Librarian")]
        public ActionResult Create()
        {
            //ViewBag.isbn = new SelectList(db.items, "isbn", "translation");
            //ViewBag.title_no = new SelectList(db.titles, "title_no", "title1");
            //ViewBag.isbn = new SelectList(db.loans, "isbn", "isbn");
            TitleViewModel titleviewmodel = TempData["TitleViewData"] as TitleViewModel;
            return View(titleviewmodel);
        }

        // POST: Copies/Create
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "isbn,title,title_no,author,translation,cover,loanable")] TitleViewModel titleviewmodel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Find the value of the maximum copy_no for specified isbn in copy table
                    short maxCopyNo = db.copies.Where(r => r.isbn == titleviewmodel.isbn).Select(r => r.copy_no).Max();
                    maxCopyNo += 1;
                    copy copy = new copy();
                    copy.isbn = titleviewmodel.isbn;
                    copy.copy_no = maxCopyNo;
                    copy.title_no = titleviewmodel.title_no;
                    copy.on_loan = "N";
                    db.copies.Add(copy);
                    db.SaveChanges();
                    TempData["UserMessageSuccess"] = "Create Copy Completed.";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            TempData["UserMessageError"] = "Create Copy Failed.";
            TempData["TitleViewData"] = titleviewmodel;
            return View(titleviewmodel);
        }

        // GET: Copies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            copy copy = db.copies.Find(id);
            if (copy == null)
            {
                return HttpNotFound();
            }
            ViewBag.isbn = new SelectList(db.items, "isbn", "translation", copy.isbn);
            ViewBag.title_no = new SelectList(db.titles, "title_no", "title1", copy.title_no);
            ViewBag.isbn = new SelectList(db.loans, "isbn", "isbn", copy.isbn);
            return View(copy);
        }

        // POST: Copies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "isbn,copy_no,title_no,on_loan")] copy copy)
        {
            if (ModelState.IsValid)
            {
                db.Entry(copy).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.isbn = new SelectList(db.items, "isbn", "translation", copy.isbn);
            ViewBag.title_no = new SelectList(db.titles, "title_no", "title1", copy.title_no);
            ViewBag.isbn = new SelectList(db.loans, "isbn", "isbn", copy.isbn);
            return View(copy);
        }

        // GET: Copies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            copy copy = db.copies.Find(id);
            if (copy == null)
            {
                return HttpNotFound();
            }
            return View(copy);
        }

        // POST: Copies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            copy copy = db.copies.Find(id);
            db.copies.Remove(copy);
            db.SaveChanges();
            return RedirectToAction("Index");
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
