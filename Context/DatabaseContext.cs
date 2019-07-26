using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AttendanceWebApi.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AttendanceWebApi.Context
{
    public partial class DatabaseContext : DbContext
    {
            
        public DatabaseContext() : base("name=DefaultConnection") { }

        public DbSet<Employee> employees { get; set; }
        public DbSet<Timeinout> timeinouts { get; set; }
        public DbSet<Adminaccount> adminaccounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }



    }
}