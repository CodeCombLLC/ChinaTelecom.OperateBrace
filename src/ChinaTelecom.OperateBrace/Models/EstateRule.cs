using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.OperateBrace.Models
{
    public class EstateRule
    {
        public Guid Id { get; set; }

        [ForeignKey("Estate")]
        public Guid EstateId { get; set; }

        public virtual Estate Estate { get; set; }

        [MaxLength(64)]
        public string Rule { get; set; }
    }
}
