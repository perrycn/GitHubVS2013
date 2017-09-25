using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibrarySite;
using LibrarySite.Controllers;
using LibrarySite.Models;
using LibrarySite.Tests.Fakes;

namespace LibrarySite.Tests.Controllers
{
    [TestClass]
    public class membersControllerTest
    {
        [TestMethod]
        public void Index()
        {
            membersController controller = new membersController();
            controller.thisIsUnitTest = true;
            controller.ControllerContext = new FakeControllerContext();

            ViewResult result = controller.Index() as ViewResult;
            IEnumerable<member> model = result.Model as IEnumerable<member>;
            Assert.AreEqual(10, model.Count());
        }

        [TestMethod]
        public void Create_Saves_Adult_When_Valid()
        {
            membersController controller = new membersController();
            controller.ControllerContext = new FakeControllerContext();
            member m1 = new member();
            m1.firstname = "George";
            string hrminsecs = DateTime.Now.ToString("HHmmss");
            m1.lastname = "Smith" + hrminsecs;

            adult adultMember = new adult();
            adultMember.member_no = m1.member_no;
            adultMember.street = "1465 Delmar";
            adultMember.city = "St. Louis";
            adultMember.state = "MO";
            adultMember.zip = "63101";

            int prevAdultCount = controller.DB.adults.Count();
            controller.CreateAdult(m1, adultMember);

            Assert.AreEqual(1, (controller.DB.adults.Count() - prevAdultCount));
        }

        [TestMethod]
        public void Create_Does_Not_Saves_Adult_When_Invalid()
        {
            membersController controller = new membersController();
            controller.ControllerContext = new FakeControllerContext();
            member m1 = new member();
            m1.firstname = "George";
            string hrminsecs = DateTime.Now.ToString("HHmmss");
            m1.lastname = "Smith" + hrminsecs;

            adult adultMember = new adult();
            adultMember.member_no = m1.member_no;
            adultMember.street = "1465 Delmar";
            adultMember.city = "St. Louis";
            adultMember.state = "MO";
            adultMember.zip = "63101";

            int prevAdultCount = controller.DB.adults.Count();
            controller.ModelState.AddModelError("", "Invalid");
            controller.CreateAdult(m1, adultMember);

            Assert.AreEqual(0, (controller.DB.adults.Count() - prevAdultCount));
        }
    }
}
