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
    
    public partial class reservation
    {
        public int isbn { get; set; }
        public short member_no { get; set; }
        public Nullable<System.DateTime> log_date { get; set; }
        public string remarks { get; set; }
    
        public virtual item item { get; set; }
        public virtual member member { get; set; }
    }
}
