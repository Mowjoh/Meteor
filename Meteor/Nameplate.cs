//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Meteor
{
    using System;
    using System.Collections.Generic;
    
    public partial class Nameplate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Nameplate()
        {
            this.SkinLibraries = new HashSet<SkinLibrary>();
        }
    
        public int Id { get; set; }
        public int character_id { get; set; }
        public string name { get; set; }
        public string hash { get; set; }
        public string author { get; set; }
    
        public virtual Character Character { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SkinLibrary> SkinLibraries { get; set; }
    }
}
