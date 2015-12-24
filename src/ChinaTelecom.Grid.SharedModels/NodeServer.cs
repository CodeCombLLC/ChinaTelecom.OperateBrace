using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.Grid.SharedModels
{
    public enum ServerStatus
    {
        在线,
        离线
    }

    public enum ServerType
    {
        主要的,
        备份
    }

    public class NodeServer
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string City { get; set; }

        [MaxLength(64)]
        public string BussinessHall { get; set; }

        public ServerType Type { get; set; }

        [NotMapped]
        public ServerStatus Status { get; set; }

        [MaxLength(512)]
        public string PrivateKey { get; set; }

        [MaxLength(128)]
        public string Server { get; set; }

        public int Port { get; set; }
    }
}
