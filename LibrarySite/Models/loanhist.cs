//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibrarySite.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class loanhist
    {
        public int isbn { get; set; }
        public short copy_no { get; set; }
        public System.DateTime out_date { get; set; }
        public int title_no { get; set; }
        public short member_no { get; set; }
        public Nullable<System.DateTime> due_date { get; set; }
        public Nullable<System.DateTime> in_date { get; set; }
        public Nullable<decimal> fine_assessed { get; set; }
        public Nullable<decimal> fine_paid { get; set; }
        public Nullable<decimal> fine_waived { get; set; }
        public string remarks { get; set; }
    
        public virtual copy copy { get; set; }
        public virtual title title { get; set; }
    }
}
