using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibrarySite.Models;

namespace LibrarySite
{
    public static class DefaultValues
    {
        public static readonly short MaxCheckOutItemLimit = 4;

        public static SelectList ItemsPerPageList
        {
            get
            {
                return (new SelectList(new List<int> { 10, 20, 40, 50, 100 }, selectedValue: 10));
            }
        }
        
        public static DateTime NewExpirationDate
        {
            get
            {
                return DateTime.Now.AddYears(1);
            }
        }

        public static SelectList StatesList
        {
            get
            {
                return (new SelectList(new List<string> { 
                                        "AL", "AK", "AR", "AZ", "CA", "CO", "CT", "DE", "FL", "GA",
                                        "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD",
                                        "ME", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", 
                                        "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC",
                                        "SD", "TN", "TX", "UT", "VA", "VT", "WA", "WI", "WV", "WY"}));
            }
        }

        public static SelectList TranslationList
        {
            get
            {
                return (new SelectList(new List<string> {
                    "ARABIC", "CHINESE", "ENGLISH", "FRENCH", "GERMAN", "JAPANESE", "KOREAN", 
                    "LATIN", "PORTUGES", "SPANISH" }));
            }
        }
    }
}