using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ChinaTelecom.OperateBrace.Models
{
    public class Estate
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public double Lon { get; set; }

        public double Lat { get; set; }

        [NotMapped]
        public int Level { get; set; }

        [MaxLength(32)]
        [Required(AllowEmptyStrings = true)]
        public string Area { get; set; }

        [JsonIgnore]
        public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

        [JsonIgnore]
        public virtual ICollection<EstateRule> Rules { get; set; } = new List<EstateRule>(); 
    }
}
