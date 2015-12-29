using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace ChinaTelecom.OperateBrace.Models
{
    public static class SampleData
    {
        public static async Task InitDB(IServiceProvider services)
        {
            var DB = services.GetRequiredService<GridContext>();
            var UserManager = services.GetRequiredService<UserManager<User>>();
            var RoleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            if (DB.Database.EnsureCreated())
            {
                await RoleManager.CreateAsync(new IdentityRole { Name = "系统管理员" });
                await RoleManager.CreateAsync(new IdentityRole { Name = "网格主管" });
                await RoleManager.CreateAsync(new IdentityRole { Name = "网格经理" });

                var user = new User { UserName = "root" };
                await UserManager.CreateAsync(user, "123456");
                await UserManager.AddToRoleAsync(user, "系统管理员");
            }
        }
    }
}
