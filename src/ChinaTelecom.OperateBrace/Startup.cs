using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Entity;
using ChinaTelecom.OperateBrace.Models;

namespace ChinaTelecom.OperateBrace
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration Config;
            var env = services.BuildServiceProvider().GetRequiredService<IApplicationEnvironment>();
            services.AddMvc();
            services.AddSmartUser<User, string>();
            services.AddSmartCookies();
            services.AddConfiguration(out Config);
            services.AddSQLFieldParser();

            if (Config["Data:DefaultConnection:Mode"] == "SQLite")
            {
                services.AddEntityFramework()
                    .AddDbContext<GridContext>(x => x.UseSqlite(Config["Data:DefaultConnection:ConnectionString"].Replace("{appRoot}", env.ApplicationBasePath)))
                    .AddSqlite();
            }
            else if (Config["Data:DefaultConnection:Mode"] == "SqlServer")
            {
                services.AddEntityFramework()
                    .AddDbContext<GridContext>(x => x.UseSqlServer(Config["Data:DefaultConnection:ConnectionString"]))
                    .AddSqlServer();
            }
            else if (Config["Data:DefaultConnection:Mode"] == "PostgreSQL")
            {
                services.AddEntityFramework()
                    .AddNpgsql()
                    .AddDbContext<GridContext>(x => x.UseNpgsql(Config["Data:DefaultConnection:ConnectionString"]));
            }
            else
            {
                services.AddEntityFramework()
                    .AddDbContext<GridContext>(x => x.UseInMemoryDatabase())
                    .AddInMemoryDatabase();
            }

            services.AddIdentity<User, IdentityRole>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonLetterOrDigit = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddEntityFrameworkStores<GridContext>()
                .AddDefaultTokenProviders();
        }

        public async void Configure(IApplicationBuilder app, ILoggerFactory logger)
        {
            logger.AddConsole(LogLevel.Warning);

            app.UseIISPlatformHandler();
            app.UseStaticFiles();
            app.UseIdentity();
            app.UseAutoAjax();
            app.UseMvcWithDefaultRoute();
            await SampleData.InitDB(app.ApplicationServices);
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
