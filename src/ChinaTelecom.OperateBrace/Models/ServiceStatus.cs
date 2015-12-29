using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.Models
{
    public enum ServiceStatus
    {
        在用,
        单向欠停,
        双向欠停,
        欠费拆机,
        用户报停,
        用户拆机,
        装机未竣工退单,
        违章停机,
        预拆机,
        未知
    }
}
