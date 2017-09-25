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
    public class TitlesController : Controller
    {
        private libraryEntities db = new libraryEntities();

        // GET: Titles
        public ActionResult Index()
        {
            return View(db.titles.ToList());
        }

        // GET: Titles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            title title = db.titles.Find(id);
            if (title == null)
            {
                return HttpNotFound();
            }
            return View(title);
        }

        // GET: Titles/Create
        [Authorize(Roles="Librarian")]
        public ActionResult Create()
        {
            TitleViewModel titleviewmodel = TempData["TitleViewData"] as TitleViewModel;
            return View(titleviewmodel);
        }

        // POST: Titles/Create
        [Authorize(Roles="Librarian")]
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateConfirmed([Bind(Include="isbn,title,author,translation,cover,loanable")] TitleViewModel titleviewmodel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var title = new title();
                    title.title1 = titleviewmodel.title;
                    title.author = titleviewmodel.author;
                    var item = new item();
                    item.isbn = titleviewmodel.isbn;
                    item.translation = titleviewmodel.translation;
                    item.cover = titleviewmodel.cover;
                    item.loanable = titleviewmodel.loanable;
                    title.items.Add(item);
                    var copy = new copy();
                    copy.isbn = titleviewmodel.isbn;
                    copy.copy_no = 1;
                    copy.on_loan = "N";
                    title.copies.Add(copy);
                    db.titles.Add(title);
                    db.SaveChanges();
                    TempData["UserMessageSuccess"] = "Create Book Completed.";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            TempData["UserMessageError"] = "Create Book Failed.";
            TempData["TitleViewData"] = titleviewmodel;
            return View(titleviewmodel);
        }

        // GET: Titles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            title title = db.titles.Find(id);
            if (title == null)
            {
                return HttpNotFound();
            }
            return View(title);
        }

        // POST: Titles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "title_no,title1,author,synopsis")] title title)
        {
            if (ModelState.IsValid)
            {
                db.Entry(title).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(title);
        }

        // GET: Titles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            title title = db.titles.Find(id);
            if (title == null)
            {
                return HttpNotFound();
            }
            return View(title);
        }

        // POST: Titles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            title title = db.titles.Find(id);
            db.titles.Remove(title);
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
