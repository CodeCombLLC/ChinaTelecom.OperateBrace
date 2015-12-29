using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.OperateBrace.Models
{
    public class Building
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public int TopLayers { get; set; }

        public int BottomLayers { get; set; }

        public int Doors { get; set; }

        public int Units { get; set; }

        [ForeignKey("Estate")]
        public Guid EstateId { get; set; }

        public virtual Estate Estate { get; set; }

        public virtual ICollection<House> Houses { get; set; } = new List<House>();
    }
}
