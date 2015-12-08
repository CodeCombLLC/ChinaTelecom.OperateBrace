using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using ChinaTelecom.Grid.Models;

namespace ChinaTelecom.Grid
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSmartUser<User, string>();

            services.AddEntityFramework()
                .AddSqlite()
                .AddDbContext<GridContext>();

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<GridContext>()
                .AddDefaultTokenProviders();
        }

        public async void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();
            app.UseIdentity();
            app.UseAutoAjax();
            app.UseMvcWithDefaultRoute();
            await SampleData.InitDB(app.ApplicationServices);
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
