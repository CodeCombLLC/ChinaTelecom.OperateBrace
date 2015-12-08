using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.Grid.Models
{
    public class Series
    {
        public Guid Id { get; set; }

        public DateTime Time { get; set; }

        public virtual ICollection<Record> Records { get; set; } = new List<Record>();
    }
}
