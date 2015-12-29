using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChinaTelecom.OperateBrace.Models
{
    public enum SeriesStatus
    {
        等待中,
        导入中,
        导入完成
    }
    public class Series
    {
        public Guid Id { get; set; }

        [NotMapped]
        public SeriesStatus Status
        {
            get
            {
                if (FailedCount == 0 && TotalCount == 0 && ImportedCount == 0 && DateTime.Now <= Time.AddMinutes(5))
                    return SeriesStatus.等待中;
                else if (FailedCount + ImportedCount >= TotalCount)
                    return SeriesStatus.导入完成;
                else
                    return SeriesStatus.导入中;
            }
        }

        public long FailedCount { get; set; }

        public long TotalCount { get; set; }

        [NotMapped]
        public long ImportedCount { get; set; }

        public DateTime Time { get; set; }

        public virtual ICollection<Record> Records { get; set; } = new List<Record>();
    }
}
