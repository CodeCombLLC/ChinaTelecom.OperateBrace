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
        public int MobileTotal { get; set; }

        [NotMapped]
        public int MobileAdded { get; set; }

        [NotMapped]
        public int MobileLeft { get; set; }

        [NotMapped]
        public int TelTotal { get; set; }

        [NotMapped]
        public int TelAdded { get; set; }

        [NotMapped]
        public int TelLeft { get; set; }

        [NotMapped]
        public int LanTotal { get; set; }

        [NotMapped]
        public int LanAdded { get; set; }

        [NotMapped]
        public int LanLeft { get; set; }

        [NotMapped]
        public int TotalCount { get; set; }

        [NotMapped]
        public int TotalAdded { get; set; }

        [NotMapped]
        public int TotalLeft { get; set; }
    }
}
