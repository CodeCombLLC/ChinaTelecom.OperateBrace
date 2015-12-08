using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ChinaTelecom.Grid.Models
{
    public class Building
    {
        public Guid Id { get; set; }
        
        [MaxLength(32)]
        public string Flag { get; set; }

        public int Layers { get; set; }

        public int Units { get; set; }

        public virtual ICollection<House> Houses { get; set; } = new List<House>();
    }
}
