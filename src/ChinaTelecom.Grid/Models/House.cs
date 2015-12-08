using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ChinaTelecom.Grid.Models
{
    public enum HouseStatus
    {
        中国电信,
        中国联通,
        中国移动,
        其他运营商,
        未装机
    }

    public class House
    {
        public Guid Id { get; set; }

        public HouseStatus HouseStatus { get; set; }

        public ServiceStatus? ServiceStatus { get; set; }

        [MaxLength(32)]
        public string FullName { get; set; }

        [MaxLength(32)]
        public string Phone { get; set; }

        public DateTime LastUpdate { get; set; }

        public Guid BuildingId { get; set; }

        public virtual Building Building { get; set; }

        public int Unit { get; set; }

        public int Layer { get; set; }

        public int Door { get; set; }
    }
}