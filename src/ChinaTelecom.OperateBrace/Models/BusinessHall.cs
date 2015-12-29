using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.OperateBrace.Models
{
    public class BusinessHall
    {
        [MaxLength(32)]
        public string Id { get; set; }

        [MaxLength(64)]
        public string Title { get; set; }

        public double Lon { get; set; }

        public double Lat { get; set; }

        [NotMapped]
        public int Level { get; set; }

        [NotMapped]
        public int Added { get; set; }

        [NotMapped]
        public int Left { get; set; }
    }
}
