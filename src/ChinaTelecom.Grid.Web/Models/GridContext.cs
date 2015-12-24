using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using ChinaTelecom.Grid.SharedModels;

namespace ChinaTelecom.Grid.Web.Models
{
    public class GridContext : IdentityDbContext<User, IdentityRole, string>
    {
        public DbSet<NodeServer> NodeServers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(e =>
            {
                e.HasIndex(x => x.City);
                e.HasIndex(x => x.BussinessHall);
            });

            builder.Entity<NodeServer>(e =>
            {
                e.HasIndex(x => x.City);
                e.HasIndex(x => x.BussinessHall);
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.Type);
            });
        }
    }
}
