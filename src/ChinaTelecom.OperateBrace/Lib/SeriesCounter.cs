using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ChinaTelecom.OperateBrace.Models;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class SeriesCounter
    {
        public static long SeriesCaculate(this IHtmlHelper self, Guid id)
        {
            var db = self.ViewContext.HttpContext.RequestServices.GetRequiredService<GridContext>();
            var cnt = db.Records
                .Where(x => x.SeriesId == id)
                .Count();
            return cnt;
        }
    }
}
