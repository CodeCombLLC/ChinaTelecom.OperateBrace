using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.Grid.Models
{
    public class Estate
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public double Lon { get; set; }

        public double Lat { get; set; }

        [MaxLength(32)]
        public string Area { get; set; }

        public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}
