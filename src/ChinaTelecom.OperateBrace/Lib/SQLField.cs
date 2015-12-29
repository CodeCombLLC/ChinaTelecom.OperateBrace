namespace ChinaTelecom.OperateBrace.Lib
{
    using Microsoft.Extensions.Configuration;
    public class SQLField
    {
        private IConfiguration Config;
        public SQLField(IConfiguration config)
        {
            Config = config;
        }

        public string Parse(string field)
        {
            if (Config["Data:DefaultConnection:Mode"] == "SQLite")
                return $@"""{field}""";
            else if (Config["Data:DefaultConnection:Mode"] == "PostgreSQL")
                return $@"""{field}""";
            else
                return $"[{field}]";
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using ChinaTelecom.OperateBrace.Lib;
    public static class SQLFieldExtensions
    {
        public static IServiceCollection AddSQLFieldParser(this IServiceCollection self)
        {
            return self.AddScoped<SQLField>();
        }
    }
}