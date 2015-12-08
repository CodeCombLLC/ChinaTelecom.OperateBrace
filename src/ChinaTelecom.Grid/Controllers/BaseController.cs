using Microsoft.AspNet.Mvc;
using ChinaTelecom.Grid.Models;

namespace ChinaTelecom.Grid.Controllers
{
    public class BaseController : BaseController<GridContext, User, string>
    {
    }
}
