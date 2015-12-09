using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.Grid.Models
{
    public class AddressMap
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string Account { get; set; }

        [ForeignKey("House")]
        public Guid HouseId { get; set; }

        public virtual House House { get; set; }
    }
}
