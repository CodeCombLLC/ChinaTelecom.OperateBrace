using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ChinaTelecom.Grid.Models
{
    public class GridContext : IdentityDbContext<User>
    {
        public DbSet<Estate> Estates { get; set; }

        public DbSet<EstateRule> EstateRules { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Estate>(e =>
            {
                e.HasIndex(x => x.Lon);
                e.HasIndex(x => x.Lat);
            });

            builder.Entity<EstateRule>(e =>
            {
                e.HasIndex(x => x.Rule);
            });
            

        }
    }
}
