using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.OperateBrace.Models
{
    public class Record
    {
        public Guid Id { get; set; }

        [MaxLength(32)]
        public string FuseIdentifier { get; set; }

        public DateTime ImportedTime { get; set; }

        [MaxLength(32)]
        public string Account { get; set; }

        public ServiceStatus Status { get; set; }

        [MaxLength(32)]
        public string CustomerName { get; set; }

        [MaxLength(32)]
        public string ContractorName { get; set; }

        [MaxLength(32)]
        public string ServiceStaff { get; set; }

        [MaxLength(32)]
        public string ContractorStruct { get; set; }

        public double CurrentMonthBill { get; set; }

        public double AgentFee { get; set; }

        public double Arrearage { get; set; }

        public double Commission { get; set; }

        [MaxLength(128)]
        public string Set { get; set; }

        [MaxLength(256)]
        public string ImplementAddress { get; set; }

        [MaxLength(256)]
        public string StandardAddress { get; set; }
        
        [ForeignKey("Series")]
        public Guid SeriesId { get; set; }

        [MaxLength(18)]
        public string PRCID { get; set; }

        [MaxLength(64)]
        public string SalesProduction { get; set; }

        [MaxLength(32)]
        public string MDS { get; set; }

        [MaxLength(32)]
        public string Phone { get; set; }

        [MaxLength(32)]
        public string BusinessHallName { get; set; }

        [MaxLength(64)]
        public string BusinessHallId { get; set; }

        public bool IsFuse { get; set; }

        public virtual Series Series { get; set; }

        public bool IsHardLink { get; set; }
    }
}
