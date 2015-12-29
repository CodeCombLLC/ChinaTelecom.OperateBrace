using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ChinaTelecom.OperateBrace.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }

        public byte[] Avatar { get; set; }
    }
}
