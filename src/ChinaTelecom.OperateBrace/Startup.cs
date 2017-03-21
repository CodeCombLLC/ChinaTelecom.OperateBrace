using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ChinaTelecom.OperateBrace.Models;

namespace ChinaTelecom.OperateBrace
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration Config;
            services.AddMvc();
            services.AddSmartUser<User, string>();
            services.AddSmartCookies();
            services.AddConfiguration(out Config);
            services.AddSQLFieldParser();

            if (Config["Data:DefaultConnection:Mode"] == "SQLite")
            {
                services.AddEntityFrameworkSqlite()
                    .AddDbContext<GridContext>(x => x.UseSqlite(Config["Data:DefaultConnection:ConnectionString"]));
            }
            else if (Config["Data:DefaultConnection:Mode"] == "SqlServer")
            {
                services.AddEntityFrameworkSqlServer()
                    .AddDbContext<GridContext>(x => x.UseSqlServer(Config["Data:DefaultConnection:ConnectionString"]));
            }
            else if (Config["Data:DefaultConnection:Mode"] == "PostgreSQL")
            {
                services.AddEntityFrameworkNpgsql()
                    .AddDbContext<GridContext>(x => x.UseNpgsql(Config["Data:DefaultConnection:ConnectionString"]));
            }
            else if (Config["Data:DefaultConnection:Mode"] == "MySQL")
            {
                services.AddEntityFrameworkMySql()
                    .AddDbContext<GridContext>(x => x.UseMySql(Config["Data:DefaultConnection:ConnectionString"]));
            }
            else
            {
                services.AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<GridContext>(x => x.UseInMemoryDatabase());
            }

            services.AddIdentity<User, IdentityRole>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddEntityFrameworkStores<GridContext>()
                .AddDefaultTokenProviders();
        }

        public async void Configure(IApplicationBuilder app, ILoggerFactory logger)
        {
            logger.AddConsole(LogLevel.Warning);
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseIdentity();
            app.UseAutoAjax();
            app.UseMvcWithDefaultRoute();
            await SampleData.InitDB(app.ApplicationServices);
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
