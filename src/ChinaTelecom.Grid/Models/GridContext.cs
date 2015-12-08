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

            builder.Entity<Building>(e =>
            {
                e.HasIndex(x => x.Units);
                e.HasIndex(x => x.Layers);
            });

            builder.Entity<House>(e =>
            {
                e.HasIndex(x => x.HouseStatus);
                e.HasIndex(x => x.LastUpdate);
                e.HasIndex(x => x.ServiceStatus);
                e.HasIndex(x => x.Layer);
                e.HasIndex(x => x.Unit);
                e.HasIndex(x => x.Door);
                e.HasIndex(x => x.Phone);
                e.HasIndex(x => x.FullName);
            });
        }
    }
}
