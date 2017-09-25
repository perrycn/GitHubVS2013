using LibrarySite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibrarySite
{
    public class Utilities
    {
        private libraryEntities db = new libraryEntities();

        public SelectList AdultMembersList
        {
            get
            {
                var membersList = db.members.OrderBy(m => m.lastname).ThenBy(m => m.firstname).ThenBy(m => m.middleinitial).ThenBy(m => m.member_no);
                List<SelectListItem> adultMembersList = new List<SelectListItem>();
                foreach (var item in membersList)
	            {
                    if (item.member_type == "Adult") {
                        adultMembersList.Add(new SelectListItem
                        {
                            Text = item.lastname + ", " + item.firstname + (item.middleinitial != null ? " " + item.middleinitial : "") + " (" + item.member_no.ToString() + ")",
                            Value = item.member_no.ToString()
                        });

                    }		 
	            }
                return new SelectList(adultMembersList, "Value", "Text");
            }
        }

        public SelectList MembersList
        {
            get
            {
                var membersList = db.members.OrderBy(m => m.lastname).ThenBy(m => m.firstname).ThenBy(m => m.middleinitial).ThenBy(m => m.member_no)
                 .Select(m => new SelectListItem()
                 {
                     Text = m.lastname + ", " + m.firstname + (m.middleinitial != null ? " " + m.middleinitial : "") + " (" + m.member_no.ToString() + ")",
                     Value = m.member_no.ToString(),
                 });
                return new SelectList(membersList, "Value", "Text");
            }
        }
    }
}