using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.Grid.Models
{
    public class BusinessHall
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string Title { get; set; }

        [MaxLength(64)]
        public string MarketingCenter { get; set; }

        public double Lon { get; set; }

        public double Lat { get; set; }

        [MaxLength(32)]
        public string City { get; set; }
    }
}
