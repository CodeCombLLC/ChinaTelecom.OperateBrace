using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ChinaTelecom.Grid.SharedModels
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }

        public byte[] Avatar { get; set; }

        [MaxLength(64)]
        public string City { get; set; }

        [MaxLength(64)]
        public string BussinessHall { get; set; }
    }
}
