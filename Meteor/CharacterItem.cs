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
    
    public partial class CharacterItem
    {
        public int Id { get; set; }
        public int character_id { get; set; }
        public string item { get; set; }
        public bool character_wide { get; set; }
    
        public virtual Character Character { get; set; }
    }
}
