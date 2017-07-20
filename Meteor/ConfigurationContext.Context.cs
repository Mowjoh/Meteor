﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class MeteorLibraryEntities : DbContext
    {
        public MeteorLibraryEntities()
            : base("name=MeteorLibraryEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<character> characters { get; set; }
        public virtual DbSet<configuration> configurations { get; set; }
        public virtual DbSet<nameplate> nameplates { get; set; }
        public virtual DbSet<Packer> Packers { get; set; }
        public virtual DbSet<skin_library> skin_library { get; set; }
        public virtual DbSet<skin> skins { get; set; }
        public virtual DbSet<workspace> workspaces { get; set; }
    
        public virtual int ResetCharacters()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ResetCharacters");
        }
    
        public virtual int ResetConfig()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ResetConfig");
        }
    
        public virtual int ResetSkinLibrary()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ResetSkinLibrary");
        }
    
        public virtual int ResetSkins()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ResetSkins");
        }
    
        public virtual int ResetWorkspaces()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("ResetWorkspaces");
        }
    
        public virtual int StartOver()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("StartOver");
        }
    }
}
