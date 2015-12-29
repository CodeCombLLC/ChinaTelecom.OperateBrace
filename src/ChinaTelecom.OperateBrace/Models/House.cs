using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.OperateBrace.Models
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

        public ServiceStatus HardlinkStatus { get; set; }

        public ServiceStatus MobileStatus { get; set; }

        [NotMapped]
        public virtual ServiceStatus ServiceStatus
        {
            get
            {
                if (IsFuse && MobileStatus != ServiceStatus.未知)
                    return (ServiceStatus)Math.Max((int)HardlinkStatus, (int)MobileStatus);
                else
                    return HardlinkStatus;
            }
        }

        [MaxLength(32)]
        public string FullName { get; set; }

        [MaxLength(32)]
        public string Phone { get; set; }

        [MaxLength(64)]
        public string Account { get; set; }

        public bool IsFuse { get; set; }

        [MaxLength(32)]
        public string FuseIdentifier { get; set; }

        public DateTime LastUpdate { get; set; }

        [ForeignKey("Building")]
        public Guid BuildingId { get; set; }

        public virtual Building Building { get; set; }

        public int Unit { get; set; }

        public int Layer { get; set; }

        public int Door { get; set; }
        
        [MaxLength(32)]
        public string BusinessHallId { get; set; }

        public bool IsStatusChanged { get; set; }
    }
}