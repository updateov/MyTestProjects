﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TalTun.WCF.WCFServiceHost.Platform
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class LabDbEntities : DbContext
    {
        public LabDbEntities()
            : base("name=LabDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<tt_WCF_Lab1_Products> tt_WCF_Lab1_Products { get; set; }
        public DbSet<tt_WCF_Lab1_SalesTrx> tt_WCF_Lab1_SalesTrx { get; set; }
    }
}
