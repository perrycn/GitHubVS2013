//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibrarySite.Data.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class copy
    {
        public copy()
        {
            this.loanhists = new HashSet<loanhist>();
        }
    
        public int isbn { get; set; }
        public short copy_no { get; set; }
        public int title_no { get; set; }
        public string on_loan { get; set; }
    
        public virtual item item { get; set; }
        public virtual title title { get; set; }
        public virtual loan loan { get; set; }
        public virtual ICollection<loanhist> loanhists { get; set; }
    }
}
