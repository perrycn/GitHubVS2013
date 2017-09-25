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
    public class ItemsController : Controller
    {
        private libraryEntities db = new libraryEntities();

        // GET: Items
        public ActionResult Index()
        {
            var items = db.items.Include(i => i.title);
            return View(items.ToList());
        }

        // GET: Items/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            item item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: Items/Create
        public ActionResult CheckISBNExists()
        {
            return View();
        }

        [Authorize(Roles="Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckISBNExists([Bind(Include = "isbn")] item item)
        {
            if (ModelState.IsValid)
            {
                int numItem = db.items.Where(r => r.isbn == item.isbn).Count();

                // If Item and Title exists then Create Copy
                if (numItem >= 1)
                {
                    var itemWithISBN = db.items.Where(r => r.isbn == item.isbn).Single();
                    var title = db.titles.Where(r => r.title_no == itemWithISBN.title_no).Single();
                    if (title == null)
                    {
                        return HttpNotFound();
                    }

                    TitleViewModel titleviewmodel = new TitleViewModel();
                    titleviewmodel.isbn = item.isbn;
                    titleviewmodel.title = title.title1;
                    titleviewmodel.title_no = itemWithISBN.title_no;
                    titleviewmodel.author = title.author;
                    titleviewmodel.translation = itemWithISBN.translation;
                    titleviewmodel.cover = itemWithISBN.cover;
                    titleviewmodel.loanable = itemWithISBN.loanable;
                    TempData["TitleViewData"] = titleviewmodel;
                    return RedirectToAction("Create", "Copies");
                }
                else  // Else Create Book
                {
                    TitleViewModel titleviewmodel = new TitleViewModel();
                    titleviewmodel.isbn = item.isbn;
                    TempData["TitleViewData"] = titleviewmodel;
                    return RedirectToAction("Create", "Titles");
                }
            }

            return View();
        }

        // GET: Items/Create
        public ActionResult Create()
        {
            ViewBag.title_no = new SelectList(db.titles, "title_no", "title1");
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "isbn,title_no,translation,cover,loanable")] item item)
        {
            if (ModelState.IsValid)
            {
                db.items.Add(item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.title_no = new SelectList(db.titles, "title_no", "title1", item.title_no);
            return View(item);
        }

        // GET: Items/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            item item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.title_no = new SelectList(db.titles, "title_no", "title1", item.title_no);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "isbn,title_no,translation,cover,loanable")] item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.title_no = new SelectList(db.titles, "title_no", "title1", item.title_no);
            return View(item);
        }

        // GET: Items/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            item item = db.items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            item item = db.items.Find(id);
            db.items.Remove(item);
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
