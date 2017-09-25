using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibrarySite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Perry's Public Library Web Site";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Perry's Public Library";

            return View();
        }
    }
}