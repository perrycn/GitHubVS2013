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
    
    public partial class member
    {
        public member()
        {
            this.loans = new HashSet<loan>();
            this.reservations = new HashSet<reservation>();
        }
    
        public short member_no { get; set; }
        public string lastname { get; set; }
        public string firstname { get; set; }
        public string middleinitial { get; set; }
        public byte[] photograph { get; set; }
    
        public virtual adult adult { get; set; }
        public virtual juvenile juvenile { get; set; }
        public virtual ICollection<loan> loans { get; set; }
        public virtual ICollection<reservation> reservations { get; set; }
    }
}
