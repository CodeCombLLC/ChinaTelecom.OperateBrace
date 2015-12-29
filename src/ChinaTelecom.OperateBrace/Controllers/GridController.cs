using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.Configuration;
using ChinaTelecom.OperateBrace.Models;
using ChinaTelecom.OperateBrace.ViewModels;

namespace ChinaTelecom.OperateBrace.Controllers
{
    [Authorize]
    public class GridController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            ViewBag.Areas = (await UserManager.GetClaimsAsync(User.Current)).
                Where(x => x.Type == "管辖片区")
                .Select(x => x.Value).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEstate(string title, double lon, double lat, string rules, string area)
        {
            var estate = new Estate
            {
                Area = area,
                Lat = lat,
                Lon = lon,
                Title = title
            };
            DB.Estates.Add(estate);
            if (!string.IsNullOrEmpty(rules))
                foreach (var x in rules.Split(','))
                    if (!string.IsNullOrEmpty(x))
                        DB.EstateRules.Add(new EstateRule { Rule = x.Trim(), EstateId = estate.Id });
            DB.SaveChanges();
            return RedirectToAction("Index", "Grid", new { lon = estate.Lon, lat = estate.Lat });
        }

        [HttpPost]
        public IActionResult ModifyPosition(Guid id, double lon, double lat)
        {
            var estate = DB.Estates.Where(x => x.Id == id).Single();
            estate.Lon = lon;
            estate.Lat = lat;
            DB.SaveChanges();
            return Content("ok");
        }

        [HttpGet]
        public IActionResult GetEstateDetail(Guid id)
        {
            var x = DB.Estates
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Single();
            var ret = new EstateMap
            {
                Title = x.Title,
                Area = x.Area,
                TotalCTUsers = DB.Houses
                        .AsNoTracking()
                        .Include(y => y.Building)
                        .Where(y => y.Building.EstateId == x.Id && y.HouseStatus == HouseStatus.中国电信)
                        .Count(),
                TotalInUsingUsers = DB.Houses
                        .AsNoTracking()
                        .Include(y => y.Building)
                        .Where(y => y.Building.EstateId == x.Id && y.HouseStatus == HouseStatus.中国电信)
                        .Where(y => y.HardlinkStatus == ServiceStatus.在用 || y.MobileStatus == ServiceStatus.在用)
                        .Count(),
                TotalNonCTUsers = DB.Houses
                        .AsNoTracking()
                        .Include(y => y.Building)
                        .Where(y => y.Building.EstateId == x.Id && y.HouseStatus != HouseStatus.中国电信 && y.HouseStatus != HouseStatus.未装机)
                        .Count(),
                AddedUsers = DB.Houses
                        .AsNoTracking()
                        .Include(y => y.Building)
                        .Where(y => y.Building.EstateId == x.Id && y.HouseStatus != HouseStatus.中国电信 && (y.HardlinkStatus == Models.ServiceStatus.在用 || y.MobileStatus == Models.ServiceStatus.在用) && y.IsStatusChanged)
                        .Count(),
                LeftUsers = DB.Houses
                        .Include(y => y.Building)
                        .Where(y => y.Building.EstateId == x.Id && y.HouseStatus != HouseStatus.中国电信 && (y.HardlinkStatus != Models.ServiceStatus.在用 || y.MobileStatus != Models.ServiceStatus.在用) && y.IsStatusChanged)
                        .Count()
            };
            if (ret.TotalCTUsers == 0)
                ret.UsingRate = 0;
            else
                ret.UsingRate = (double)ret.TotalInUsingUsers / (double)ret.TotalCTUsers;
            return Json(ret);
        }

        [HttpGet]
        public IActionResult GetEstates(double left, double right, double top, double bottom, [FromServices] IConfiguration Config)
        {
            var estates = DB.Estates
                .AsNoTracking()
                .Where(x => x.Lon >= left && x.Lon <= right && x.Lat <= top && x.Lat >= bottom)
                .ToList();
            var id = estates
                .Select(x => x.Id)
                .ToList();
            var tmp = DB.Buildings
                .Include(x => x.Houses)
                .Include(x => x.Estate)
                .AsNoTracking()
                .GroupBy(x => x.EstateId)
                .ToList()
                .Select(x => new
                {
                    Key = x.Key,
                    LeftCount = x.Sum(y => y.Houses.Where(z => id.Contains(z.Building.EstateId)
                        && z.IsStatusChanged == true
                        && z.HouseStatus == HouseStatus.中国电信
                        && z.ServiceStatus != ServiceStatus.在用).Count()),
                    AddedCount = x.Sum(y => y.Houses.Where(z => id.Contains(z.Building.EstateId)
                        && z.IsStatusChanged == true
                        && z.HouseStatus == HouseStatus.中国电信
                        && z.ServiceStatus == ServiceStatus.在用).Count())
                })
                .ToList();
            foreach (var x in estates)
            {
                if (!tmp.Any(a => a.Key == x.Id))
                    x.Level = 3;
                else
                {
                    var s = tmp.Where(a => a.Key == x.Id).Single();
                    if (s.LeftCount == 0)
                        x.Level = 0;
                    else if (s.LeftCount <= Convert.ToInt32(Config["Settings:Threshold:BusinessHall:Yellow"]))
                        x.Level = 1;
                    else
                        x.Level = 2;
                    if (x.Level == 0 && s.AddedCount >= Convert.ToInt32(Config["Settings:Threshold:BusinessHall:Cyan"]))
                        x.Level = 5;
                    if (x.Level == 1 && s.AddedCount >= Convert.ToInt32(Config["Settings:Threshold:BusinessHall:Cyan"]))
                        x.Level = 4;
                }
            }
            return Json(estates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEstate(Guid id)
        {
            var estate = DB.Estates
                .Where(x => x.Id == id)
                .Single();
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(estate.Area))
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您无权删除该小区";
                    });
            }
            DB.Estates.Remove(estate);
            DB.SaveChanges();
            return RedirectToAction("Estate", "Grid");
        }

        [HttpGet]
        public async Task<IActionResult> Area(bool? xls)
        {
            IEnumerable<Estate> tmp = DB.Estates
                .Include(x => x.Buildings)
                .ThenInclude(x => x.Houses);
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                tmp = tmp.Where(x => areas.Contains(x.Area));
            }
            var ret = tmp.OrderBy(x => x.Area)
                .GroupBy(x => x.Area)
                .Select(x => new Area
                {
                    Id = x.Key,
                    Count = x.Count(),
                    CTUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信).Count())),
                    CTInUsingUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信 && (a.HardlinkStatus == Models.ServiceStatus.在用 || a.MobileStatus == Models.ServiceStatus.在用)).Count())),
                    NonCTUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus != HouseStatus.中国电信 && a.HouseStatus != HouseStatus.未装机).Count())),
                    AddedUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信 && a.IsStatusChanged && (a.HardlinkStatus == Models.ServiceStatus.在用 || a.MobileStatus == Models.ServiceStatus.在用)).Count())),
                    LeftUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信 && a.IsStatusChanged && (a.HardlinkStatus != Models.ServiceStatus.在用 || a.MobileStatus != Models.ServiceStatus.在用)).Count())),
                    Lon = x.FirstOrDefault() == null ? null : (double?)x.FirstOrDefault().Lon,
                    Lat = x.FirstOrDefault() == null ? null : (double?)x.FirstOrDefault().Lat
                });
            ViewBag.AreaCount = ret.Count();
            if (xls.HasValue && xls.Value)
                return XlsView(ret.ToList(), "Area.xls", "ExportArea");
            else
                return PagedView(ret);
        }

        [HttpGet]
        public async Task<IActionResult> Estate(string Area, string Title, string Circle, bool? raw)
        {
            IEnumerable<Estate> ret = DB.Estates
                .Include(x => x.Buildings)
                .ThenInclude(x => x.Houses)
                .Include(x => x.Rules);
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                ret = ret.Where(x => areas.Contains(x.Area));
            }
            if (!string.IsNullOrEmpty(Area))
                ret = ret.Where(x => x.Area == Area);
            if (!string.IsNullOrEmpty(Title))
                ret = ret.Where(x => x.Title.Contains(Title));
            if (!string.IsNullOrEmpty(Circle))
            {
                var points = Lib.Circle.StringToPoints(Circle);
                var edge = Lib.Circle.GetEdge(points);
                var estates = DB.Estates
                    .Where(x => x.Lat >= edge.MinLat && x.Lat <= edge.MaxLat && x.Lon >= edge.MinLon && x.Lon <= edge.MaxLon)
                    .Select(x => new
                    {
                        Id = x.Id,
                        Lon = x.Lon,
                        Lat = x.Lat
                    })
                    .ToList();
                var estateIds = estates
                    .Where(x => Lib.Circle.PointInFences(new Lib.Point { X = x.Lon, Y = x.Lat }, points)).Select(x => x.Id)
                    .ToList();
                ret = ret.Where(x => estateIds.Contains(x.Id));
            }
            ret = ret.OrderBy(x => x.Area);
            ViewBag.EstateCount = ret.Count();
            if (raw.HasValue && raw.Value)
                return XlsView(ret.ToList(), "Estate.xls", "ExportEstate");
            else
                return PagedView(ret);
        }

        [HttpGet]
        public IActionResult Show(Guid id)
        {
            ViewBag.EstateTitle = DB.Estates
                .Where(x => x.Id == id)
                .Single()
                .Title;
            var ret = DB.Buildings
                .Include(x => x.Houses)
                .Where(x => x.EstateId == id)
                .ToList();
            ret.Sort((a, b) =>
            {
                try
                {
                    if (Convert.ToInt32(a.Title) < Convert.ToInt32(b.Title))
                        return -1;
                    else if (Convert.ToInt32(a.Title) == Convert.ToInt32(b.Title))
                        return 0;
                    else
                        return 1;
                }
                catch
                {
                    return string.Compare(a.Title, b.Title);
                }
            });
            return View(ret);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveBuilding(Guid id)
        {
            var building = DB.Buildings
                .Include(x => x.Estate)
                .Where(x => x.Id == id)
                .Single();
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(building.Estate.Area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您无权删除该楼座";
                    });
                }
            }
            DB.Buildings.Remove(building);
            DB.SaveChanges();
            return RedirectToAction("Show", "Grid", new { id = building.EstateId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBuilding(Guid id, Building Model)
        {
            var estate = DB.Estates
                .Where(x => x.Id == id)
                .Single();
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(estate.Area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "创建失败";
                        x.Details = "您没有该片区的管辖权";
                    });
                }
            }
            Model.Id = Guid.NewGuid();
            Model.EstateId = id;
            DB.Buildings.Add(Model);
            DB.SaveChanges();
            return RedirectToAction("Building", "Grid", new { id = Model.Id });
        }

        [HttpGet]
        public IActionResult Building(Guid id)
        {
            var ret = DB.Buildings
                .Include(x => x.Estate)
                .Include(x => x.Houses)
                .Where(x => x.Id == id)
                .Single();
            var accounts = DB.Houses
                .Select(x => x.Account)
                .ToList();
            var rules = DB.EstateRules
                .Where(x => x.EstateId == ret.EstateId)
                .Select(x => x.Rule)
                .ToList();

            // 查找没有对应至楼宇中的地址信息
            var pendingAddress = new List<Record>();
            foreach (var x in rules)
            {
                pendingAddress.AddRange(DB.Records
                    .Where(a => !DB.Houses
                    .Select(b => b.Account)
                    .Contains(a.Account))
                    .Where(a => a.ImplementAddress.Contains(x) || a.StandardAddress.Contains(x)));
            }
            pendingAddress = pendingAddress.OrderByDescending(x => x.ImportedTime).DistinctBy(x => x.Account).ToList();

            // 尝试根据规则进行对应
            for (var i = 0; i < pendingAddress.Count; i++)
            {
                var x = pendingAddress[i];
                try
                {
                    var building = Lib.AddressAnalyser.GetBuildingNumber(x.ImplementAddress);
                    var unit = Lib.AddressAnalyser.GetUnit(x.ImplementAddress);
                    var layer = Lib.AddressAnalyser.GetLayer(x.ImplementAddress);
                    var door = Lib.AddressAnalyser.GetDoor(x.ImplementAddress);
                    if (ret.Title == building)
                    {
                        var _building = ret;
                        if (unit > 0 && unit < _building.Units && layer >= _building.BottomLayers && layer <= _building.TopLayers && door > 0 && door < _building.Doors)
                        {
                            var prev = DB.Records
                                .Where(a => a.Account == x.Account)
                                .Where(a => a.ImportedTime < x.ImportedTime)
                                .LastOrDefault();

                            // 如果已经存在用户信息则不能创建关联
                            if (DB.Houses
                                    .Where(a => a.BuildingId == _building.Id
                                        && a.Unit == unit.Value
                                        && a.Layer == layer.Value
                                        && a.Door == door.Value)
                                    .Count() > 0)
                                continue;

                            DB.Houses.Add(new House
                            {
                                Account = x.Account,
                                HardlinkStatus = x.Status,
                                HouseStatus = HouseStatus.中国电信,
                                Unit = unit.Value,
                                Layer = layer.Value,
                                Door = door.Value,
                                LastUpdate = x.ImportedTime,
                                Phone = x.Phone,
                                FullName = x.CustomerName,
                                BuildingId = _building.Id,
                                IsStatusChanged = prev == null ? true : prev.Status == x.Status ? false : true
                            });
                            DB.SaveChanges();
                            pendingAddress.Remove(x);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            ViewBag.PendingAddresses = pendingAddress;
            return View(ret);
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(string Account, Guid? BuildingId, int? Unit, int? Layer, int? Door)
        {
            if (Unit == null || Layer == null || Door == null || BuildingId == null)
            {
                var house = DB.Houses
                    .Include(x => x.Building)
                    .ThenInclude(x => x.Estate)
                    .Where(x => x.Account == Account)
                    .Single();
                if (!User.IsInRole("系统管理员"))
                {
                    var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                    if (!areas.Contains(house.Building.Estate.Area))
                    {
                        return Prompt(x =>
                        {
                            x.Title = "操作失败";
                            x.Details = "您没有权限将该楼座中的用户移出！";
                        });
                    }
                }
                DB.Houses.Remove(house);
            }
            else
            {
                var building = DB.Buildings
                    .Include(x => x.Estate)
                    .Where(x => x.Id == BuildingId.Value)
                    .Single();

                if (DB.Houses.Where(x => x.BuildingId == building.Id && x.Unit == Unit.Value && x.Layer == Layer.Value && x.Door == Door.Value).Count() > 0)
                    return Content("failed");

                if (!User.IsInRole("系统管理员"))
                {
                    var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                    if (!areas.Contains(building.Estate.Area))
                    {
                        return Prompt(x =>
                        {
                            x.Title = "操作失败";
                            x.Details = "您没有权限向该楼座添加用户！";
                        });
                    }
                }

                var record = DB.Records.Where(x => x.Account == Account && x.IsHardLink).LastOrDefault();
                var status = record.Status;
                var record2 = DB.Records.Where(x => x.Account == Account && !x.IsHardLink && x.SeriesId == record.SeriesId).LastOrDefault();
                if (record2 != null)
                    status = (ServiceStatus)Math.Max((int)status, (int)record2.Status);
                var houses = DB.Houses.Where(x => x.Account == Account).SingleOrDefault();

                if (record != null && !User.IsInRole("系统管理员") && record.ServiceStaff != User.Current.FullName && record.ContractorName != User.Current.FullName)
                    return Prompt(x =>
                    {
                        x.Title = "操作失败";
                        x.Details = "您没有权限执行该操作！";
                    });

                if (houses == null)
                {
                    DB.Houses.Add(new House
                    {
                        Account = Account,
                        BuildingId = BuildingId.Value,
                        Door = Door.Value,
                        Layer = Layer.Value,
                        Unit = Unit.Value,
                        FullName = record.CustomerName,
                        HouseStatus = HouseStatus.中国电信,
                        IsStatusChanged = true,
                        Phone = record.Phone,
                        LastUpdate = DateTime.Now,
                        HardlinkStatus = status
                    });
                }
                else
                {
                    houses.Unit = Unit.Value;
                    houses.Layer = Layer.Value;
                    houses.Door = Door.Value;
                }
            }
            DB.SaveChanges();
            return Content("ok");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHouse(Guid id, int unit, int layer, int door, string account, HouseStatus provider, string fullname, string phone, [FromHeader] string Referer)
        {
            var building = DB.Buildings
                    .Include(x => x.Estate)
                    .Where(x => x.Id == id)
                    .Single();

            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(building.Estate.Area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "操作失败";
                        x.Details = "您没有权限向该楼座添加用户！";
                    });
                }
            }

            if (provider == HouseStatus.中国电信)
            {
                var record = DB.Records
                    .Where(x => x.Account == account && x.IsFuse)
                    .LastOrDefault();
                if (record == null)
                    return Prompt(x =>
                    {
                        x.Title = "操作失败";
                        x.Details = "没有找到该接入号对应的用户！";
                        x.StatusCode = 404;
                    });
                var status = record.Status;
                var record2 = DB.Records.Where(x => x.Account == account && !x.IsHardLink && x.SeriesId == record.SeriesId).LastOrDefault();
                if (record2 != null)
                    status = (ServiceStatus)Math.Max((int)status, (int)record2.Status);

                if (!User.IsInRole("系统管理员") && record.ServiceStaff != User.Current.FullName && record.ContractorName != User.Current.FullName)
                    return Prompt(x =>
                    {
                        x.Title = "操作失败";
                        x.Details = "您没有权限执行该操作！";
                    });

                DB.Houses.Add(new House
                {
                    BuildingId = id,
                    Account = account,
                    Unit = unit,
                    Layer = layer,
                    Door = door,
                    HouseStatus = HouseStatus.中国电信,
                    LastUpdate = DateTime.Now,
                    IsStatusChanged = true,
                    Phone = record.Phone,
                    HardlinkStatus = status,
                    IsFuse = record.IsFuse,
                    MobileStatus = record2 != null ? record2.Status : ServiceStatus.未知,
                    FullName = record.CustomerName
                });
            }
            else
            {
                DB.Houses.Add(new House
                {
                    BuildingId = id,
                    Account = account,
                    Unit = unit,
                    Layer = layer,
                    Door = door,
                    HouseStatus = provider,
                    LastUpdate = DateTime.Now,
                    IsStatusChanged = true,
                    Phone = phone,
                    HardlinkStatus = ServiceStatus.未知,
                    MobileStatus = ServiceStatus.未知,
                    IsFuse = false,
                    FullName = fullname
                });
            }
            DB.SaveChanges();
            return RedirectToAction("Building", "Grid", new { id = id, unit = unit });
        }

        [HttpPost]
        public async Task<IActionResult> Move(Guid id, Guid? BuildingId, int? Unit, int? Layer, int? Door, string FullName, string Phone, HouseStatus? Provider)
        {
            var house = DB.Houses
                .Include(x => x.Building)
                .Where(x => x.Id == id)
                .SingleOrDefault();
            if (house == null)
            {
                var building = DB.Buildings
                    .Include(x => x.Estate)
                    .Where(x => x.Id == BuildingId.Value)
                    .Single();
                if (!User.IsInRole("系统管理员"))
                {
                    var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                    if (!areas.Contains(building.Estate.Area))
                    {
                        return Prompt(x =>
                        {
                            x.Title = "操作失败";
                            x.Details = "您没有权限向该楼座添加用户！";
                        });
                    }
                }
            }
            else
            {
                var building = house.Building;

                if (!User.IsInRole("系统管理员"))
                {
                    var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                    if (!areas.Contains(building.Estate.Area))
                    {
                        return Prompt(x =>
                        {
                            x.Title = "操作失败";
                            x.Details = "您没有权限向该楼座添加用户！";
                        });
                    }
                }
            }

            if (Unit == null || Layer == null || Door == null || BuildingId == null)
            {
                if (house != null)
                {
                    DB.Houses.Remove(house);
                    DB.SaveChanges();
                }
            }
            else
            {
                if (house == null)
                {
                    house = new House
                    {
                        Id = id,
                        HardlinkStatus = ServiceStatus.未知,
                        MobileStatus = ServiceStatus.未知,
                        IsFuse = false,
                        HouseStatus = Provider.Value,
                        LastUpdate = DateTime.Now,
                        IsStatusChanged = true,
                        BuildingId = BuildingId.Value,
                        Phone = Phone,
                        FullName = FullName,
                        Door = Door.Value,
                        Layer = Layer.Value,
                        Unit = Unit.Value,
                        Account = null
                    };
                    DB.Houses.Add(house);
                    DB.SaveChanges();
                }
                else
                {
                    if (DB.Houses.Where(x => x.BuildingId == BuildingId.Value && x.Unit == Unit.Value && x.Layer == Layer.Value && x.Door == Door.Value).Count() > 0)
                        return Content("failed");
                }
            }
            return Content("ok");
        }

        [HttpGet]
        public IActionResult Statistics()
        {
            ViewBag.Areas = DB.Estates
                .AsNoTracking()
                .OrderBy(x => x.Area)
                .Select(x => x.Area)
                .Distinct()
                .ToList();
            ViewBag.Contractors = DB.Records
                .AsNoTracking()
                .Select(x => x.ContractorName)
                .Distinct()
                .ToList();
            ViewBag.Staff = DB.Records
                .AsNoTracking()
                .Select(x => x.ServiceStaff)
                .Distinct()
                .ToList();
            ViewBag.Sets = DB.Records
                .AsNoTracking()
                .Select(x => x.Set)
                .Distinct()
                .ToList();
            return View();
        }

        public IActionResult GenerateStatistics(string[] Area, string[] Set, string[] Contractor, string Circle, [FromServices] IConfiguration Config)
        {
            for (var i = 0; i < Area.Count(); i++)
                if (Area[i] == null)
                    Area[i] = "";

            List<House> houses;
            if (string.IsNullOrEmpty(Circle))
            {
                houses = DB.Houses
                    .AsNoTracking()
                    .Include(x => x.Building)
                    .ThenInclude(x => x.Estate)
                    .ToList()
                    .Where(x => Area.Contains(x.Building.Estate.Area))
                    .ToList();
            }
            else
            {
                var points = Lib.Circle.StringToPoints(Circle);
                var edge = Lib.Circle.GetEdge(points);
                var estates = DB.Estates
                    .Where(x => x.Lat >= edge.MinLat && x.Lat <= edge.MaxLat && x.Lon >= edge.MinLon && x.Lon <= edge.MaxLon)
                    .Select(x => new
                    {
                        Id = x.Id,
                        Lon = x.Lon,
                        Lat = x.Lat
                    })
                    .ToList();
                var estateIds = estates
                    .Where(x => Lib.Circle.PointInFences(new Lib.Point { X = x.Lon, Y = x.Lat }, points))
                    .Select(x => x.Id)
                    .ToList();
                if (Config["Data:DefaultConnection:Mode"] != "SQLite")
                {
                    var buildingIds = DB.Buildings
                    .Where(x => estateIds.Contains(x.EstateId))
                    .Select(x => x.Id)
                    .ToList();
                    houses = DB.Houses
                        .Include(x => x.Building)
                        .ThenInclude(x => x.Estate)
                        .Where(x => buildingIds.Contains(x.BuildingId))
                        .ToList();
                }
                else
                {
                    var houseIds = new List<Guid>();
                    houses = new List<House>();
                    foreach (var x in estateIds)
                        houses.AddRange(DB.Houses
                            .Include(a => a.Building)
                            .ThenInclude(a => a.Estate)
                            .Where(a => a.Building.EstateId == x)
                            .ToList());
                }
            }

            var tmp = houses.Select(x => x.Account).ToList();
            List<Record> records;
            if (string.IsNullOrEmpty(Circle))
            {
                records = DB.Records
                .AsNoTracking()
                .Where(x => Set.Contains(x.Set)
                    && (Contractor.Contains(x.ContractorName) || Contractor.Contains(x.ServiceStaff))
                    && tmp.Contains(x.Account))
                .ToList();
            }
            else
            {
                records = DB.Records
                    .Where(x => tmp.Contains(x.Account))
                    .ToList();
            }

            ViewBag.UserStatistics = Lib.Counting.Count(records, houses);
            ViewBag.SetStatistics = records
                .OrderByDescending(x => x.ImportedTime)
                .DistinctBy(x => x.Account)
                .GroupBy(x => x.Set)
                .Select(x => new BarChartItem
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .ToList();

            List<string> area;
            if (string.IsNullOrEmpty(Circle))
            {
                area = DB.Estates
                    .AsNoTracking()
                    .Where(x => Area.Contains(x.Area))
                    .DistinctBy(x => x.Area)
                    .Select(x => x.Area)
                    .ToList();
            }
            else
            {
                var points = Lib.Circle.StringToPoints(Circle);
                var edge = Lib.Circle.GetEdge(points);
                var estates = DB.Estates
                    .Where(x => x.Lat >= edge.MinLat && x.Lat <= edge.MaxLat && x.Lon >= edge.MinLon && x.Lon <= edge.MaxLon)
                    .Select(x => new
                    {
                        Id = x.Id,
                        Lon = x.Lon,
                        Lat = x.Lat
                    })
                    .ToList();
                var estateIds = estates
                    .Where(x => Lib.Circle.PointInFences(new Lib.Point { X = x.Lon, Y = x.Lat }, points)).Select(x => x.Id)
                    .ToList();
                area = new List<string>();
                foreach (var y in estateIds)
                    area.AddRange(DB.Estates
                        .Where(x => x.Id == y)
                        .DistinctBy(x => x.Area)
                        .Select(x => x.Area)
                        .ToList());
            }

            var areaStatistics = new List<BarChartItem>();
            foreach (var x in area)
            {
                areaStatistics.Add(new BarChartItem
                {
                    Key = x,
                    Count = houses
                        .Where(a => a.Building.Estate.Area == x && a.HouseStatus == Models.HouseStatus.中国电信 && (a.HardlinkStatus == Models.ServiceStatus.在用 || a.MobileStatus == Models.ServiceStatus.在用))
                        .Count()
                });
            }
            ViewBag.AreaStatistics = areaStatistics;

            ViewBag.ContractorStatistics = records
                .Where(x => x.Status == Models.ServiceStatus.在用)
                .OrderByDescending(x => x.ImportedTime)
                .DistinctBy(x => x.Account)
                .GroupBy(x => x.ContractorName)
                .Select(x => new BarChartItem
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .ToList();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var estate = DB.Estates
                .Include(x => x.Rules)
                .Where(x => x.Id == id)
                .Single();
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(estate.Area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您无权删除该楼座";
                    });
                }
            }
            var rulesStr = "";
            foreach (var x in estate.Rules)
                rulesStr += x.Rule + ",";
            rulesStr = rulesStr.TrimEnd(',');
            ViewBag.Rules = rulesStr;
            return View(estate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string rules, string title, double lon, double lat, string area)
        {
            if (area == null)
                area = "";
            var estate = DB.Estates
                .Include(x => x.Rules)
                .Where(x => x.Id == id)
                .Single();
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(estate.Area) || !areas.Contains(area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您无权删除该楼座";
                    });
                }
            }
            estate.Title = title;
            estate.Lon = lon;
            estate.Lat = lat;
            estate.Area = area;
            foreach (var x in estate.Rules)
                DB.EstateRules.Remove(x);
            foreach (var x in rules.TrimEnd(' ').TrimEnd(',').Split(','))
                if (!string.IsNullOrEmpty(x))
                    DB.EstateRules.Add(new EstateRule
                    {
                        EstateId = estate.Id,
                        Rule = x
                    });
            DB.SaveChanges();
            return RedirectToAction("Estate", "Grid");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBuilding(Guid id, string title, int units, int toplayers, int bottomlayers, int doors, [FromHeader] string Referer)
        {
            var building = DB.Buildings
                .Include(x => x.Estate)
                .Include(x => x.Houses)
                .Where(x => x.Id == id)
                .Single();
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(building.Estate.Area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您无权删除该楼座";
                    });
                }
            }
            building.Title = title;
            building.Units = units;
            building.TopLayers = toplayers;
            building.BottomLayers = bottomlayers;
            building.Doors = doors;
            DB.SaveChanges();
            var missed = building.Houses
                .Where(x => x.Unit > units || x.Layer < bottomlayers || x.Layer > toplayers || x.Door > doors)
                .ToList();
            foreach (var x in missed)
                DB.Houses.Remove(x);
            DB.SaveChanges();
            return Redirect(Referer);
        }

        public async Task<IActionResult> Customer(string account,
            string fullname,
            ServiceStatus? status,
            bool change,
            string area,
            string estate,
            HouseStatus? provider,
            string building,
            int? unit,
            int? layer,
            int? door,
            string Circle,
            bool? raw,
            [FromServices] IConfiguration Config)
        {
            IEnumerable<House> ret = DB.Houses
                .Include(x => x.Building)
                .ThenInclude(x => x.Estate);
            if (Config["Data:DefaultConnection:Mode"] != "SQLite")
                ret = (ret as Microsoft.Data.Entity.Query.IIncludableQueryable<House, Estate>).AsNoTracking();
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                ret = ret.Where(x => areas.Contains(x.Building.Estate.Area));
            }
            if (!string.IsNullOrEmpty(account))
                ret = ret.Where(x => x.Account == account);
            if (!string.IsNullOrEmpty(fullname))
                ret = ret.Where(x => x.FullName == fullname);
            if (status.HasValue)
                ret = ret.Where(x => x.HardlinkStatus == status.Value || x.MobileStatus == status.Value);
            if (change)
                ret = ret.Where(x => x.IsStatusChanged);
            if (!string.IsNullOrEmpty(area))
                ret = ret.Where(x => x.Building.Estate.Area == area);
            if (!string.IsNullOrEmpty(estate))
            {
                var estateId = DB.EstateRules
                    .Include(x => x.Estate)
                    .Where(x => x.Rule.Contains(estate))
                    .Select(x => x.EstateId)
                    .FirstOrDefault();
                ret = ret.Where(x => x.Building.EstateId == estateId);
            }
            if (provider.HasValue)
                ret = ret.Where(x => x.HouseStatus == provider.Value);
            if (!string.IsNullOrEmpty(building))
                ret = ret.Where(x => x.Building.Title == building);
            if (unit.HasValue)
                ret = ret.Where(x => x.Unit == unit.Value);
            if (layer.HasValue)
                ret = ret.Where(x => x.Layer == layer.Value);
            if (door.HasValue)
                ret = ret.Where(x => x.Door == door.Value);
            if (!string.IsNullOrEmpty(Circle))
            {
                var points = Lib.Circle.StringToPoints(Circle);
                var edge = Lib.Circle.GetEdge(points);
                var estates = DB.Estates
                    .Where(x => x.Lat >= edge.MinLat && x.Lat <= edge.MaxLat && x.Lon >= edge.MinLon && x.Lon <= edge.MaxLon)
                    .Select(x => new
                    {
                        Id = x.Id,
                        Lon = x.Lon,
                        Lat = x.Lat
                    })
                    .ToList();
                var estateIds = estates
                    .Where(x => Lib.Circle.PointInFences(new Lib.Point { X = x.Lon, Y = x.Lat }, points))
                    .Select(x => x.Id)
                    .ToList();
                if (Config["Data:DefaultConnection:Mode"] != "SQLite")
                {
                    var buildingIds = DB.Buildings
                    .Where(x => estateIds.Contains(x.EstateId))
                    .Select(x => x.Id)
                    .ToList();
                    ret = ret.Where(x => buildingIds.Contains(x.BuildingId));
                }
                else
                {
                    var houseIds = new List<Guid>();
                    foreach (var x in estateIds)
                        houseIds.AddRange(DB.Houses
                            .AsNoTracking()
                            .Include(a => a.Building)
                            .Where(a => a.Building.EstateId == x)
                            .Select(a => a.Id)
                            .ToList());
                    ret = ret.Where(x => houseIds.Contains(x.Id));
                }
            }
            ViewBag.TotalCustomerCount = ret.Count();
            if (raw.HasValue && raw.Value)
                return XlsView(ret, "customers.xls", "ExportCustomer");
            else
                return PagedView(ret);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveHouse(Guid id, [FromHeader] string Referer)
        {
            var house = DB.Houses
                .Include(x => x.Building)
                .ThenInclude(x => x.Estate)
                .Where(x => x.Id == id)
                .Single();

            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(house.Building.Estate.Area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您无权删除该用户对应关系";
                    });
                }
            }
            if (!string.IsNullOrEmpty(house.Account))
            {
                var record = DB.Records
                .Where(x => x.Account == house.Account)
                .LastOrDefault();
                if (record == null || (record.ContractorName != User.Current.FullName && record.ServiceStaff != User.Current.FullName && !User.IsInRole("系统管理员")))
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您无权删除该用户对应关系";
                    });
                }
            }
            DB.Houses.Remove(house);
            DB.SaveChanges();
            return Redirect(Referer);
        }

        [HttpGet]
        public IActionResult Relation(string address, bool? raw)
        {
            var houses = DB.Houses
                .Select(x => x.Account)
                .Distinct();
            IEnumerable<Record> ret = DB.Records
                .AsNoTracking()
                .Where(x => !houses.Contains(x.Account));
            if (!string.IsNullOrEmpty(address))
                ret = ret.Where(x => x.ImplementAddress.Contains(address) || x.StandardAddress.Contains(address));
            if (User.IsInRole("网格经理"))
                ret = ret.Where(x => x.ServiceStaff == User.Current.FullName || x.ContractorName == User.Current.FullName);
            ret = ret.OrderByDescending(x => x.Account)
                .DistinctBy(x => x.Account);
            if (raw.HasValue && raw.Value)
                return XlsView(ret, "non-relation-customers.xls", "ExportRelation");
            else
                return PagedView(ret);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mapping(string estate, string building, Guid id, int unit, int layer, int door, string account, [FromHeader] string Referer)
        {
            if (DB.Houses.Where(x => x.Account == account).Count() > 0)
            {
                return Prompt(x =>
                {
                    x.Title = "操作失败";
                    x.Details = "该用户已经存在关联关系！";
                });
            }

            var estateId = DB.EstateRules
                    .Include(x => x.Estate)
                    .Where(x => x.Rule.Contains(estate))
                    .Select(x => x.EstateId)
                    .FirstOrDefault();

            if (estateId == null)
            {
                return Prompt(x =>
                {
                    x.Title = "操作失败";
                    x.Details = "没有找到该小区！";
                });
            }

            var _building = DB.Buildings
                    .Include(x => x.Estate)
                    .Where(x => x.EstateId == estateId && x.Title == building)
                    .SingleOrDefault();

            if (_building == null)
            {
                return Prompt(x =>
                {
                    x.Title = "操作失败";
                    x.Details = "没有找到该楼座！";
                });
            }

            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                if (!areas.Contains(_building.Estate.Area))
                {
                    return Prompt(x =>
                    {
                        x.Title = "操作失败";
                        x.Details = "您没有权限向该楼座添加用户！";
                    });
                }
            }

            var record = DB.Records
                .Where(x => x.Account == account && x.IsHardLink)
                .Last();

            var status = record.Status;
            var record2 = DB.Records.Where(x => x.Account == account && !x.IsHardLink && x.SeriesId == record.SeriesId).LastOrDefault();
            if (record2 != null)
                status = (ServiceStatus)Math.Max((int)status, (int)record2.Status);


            if (User.IsInRole("网格经理") && record.ServiceStaff != User.Current.FullName && record.ContractorName != User.Current.FullName)
            {
                return Prompt(x =>
                {
                    x.Title = "操作失败";
                    x.Details = "您没有权限将该用户映射至楼宇！";
                });
            }

            DB.Houses.Add(new House
            {
                BuildingId = _building.Id,
                Account = account,
                Unit = unit,
                Layer = layer,
                Door = door,
                HouseStatus = HouseStatus.中国电信,
                LastUpdate = DateTime.Now,
                IsStatusChanged = true,
                Phone = record.Phone,
                HardlinkStatus = record.Status,
                MobileStatus = record2 != null ? record2.Status : ServiceStatus.未知,
                IsFuse = record.IsFuse,
                FullName = record.CustomerName
            });
            DB.SaveChanges();
            return Redirect(Referer);
        }

        [HttpGet]
        [AnyRoles("系统管理员")]
        public IActionResult Assign(string Circle)
        {
            var points = Lib.Circle.StringToPoints(Circle);
            var edge = Lib.Circle.GetEdge(points);
            var estates = DB.Estates
                .Where(x => x.Lat >= edge.MinLat && x.Lat <= edge.MaxLat && x.Lon >= edge.MinLon && x.Lon <= edge.MaxLon)
                .ToList();
            var ret = estates
                .Where(x => Lib.Circle.PointInFences(new Lib.Point { X = x.Lon, Y = x.Lat }, points))
                .ToList();
            return View(ret);
        }

        [HttpPost]
        [AnyRoles("系统管理员")]
        [ValidateAntiForgeryToken]
        public IActionResult Assign(Guid[] estates, string area, [FromServices] IConfiguration Config)
        {
            if (area == null)
                area = "";
            Estate first = null;
            if (Config["Data:DefaultConnection:Mode"] != "SQLite")
            {
                var tmp = DB.Estates
                .Where(x => estates.Contains(x.Id))
                .ToList();
                foreach (var x in tmp)
                    x.Area = area;
                first = tmp.FirstOrDefault();
                DB.SaveChanges();
            }
            else
            {
                foreach(var x in estates)
                {
                    var y = DB.Estates
                        .Where(z => z.Id == x)
                        .SingleOrDefault();
                    if (first == null)
                        first = y;
                    y.Area = area;
                    DB.SaveChanges();
                }
            }
            return Prompt(x =>
            {
                x.Title = "片区指派成功";
                x.Details = $"共{estates.Count()}个小区被指派到了{area}片区中";
                x.RedirectText = "返回地图";
                if (first == null)
                    x.RedirectUrl = Url.Action("Index", "Grid");
                else
                    x.RedirectUrl = Url.Action("Index", "Grid", new { lon = first.Lon, first.Lat });
            });
        }

        public IActionResult BusinessHall()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetBusinessHalls(double left, double right, double top, double bottom, [FromServices] IConfiguration Config)
        {
            var bhs = DB.BusinessHalls
                .Where(x => x.Lon >= left && x.Lon <= right && x.Lat <= top && x.Lat >= bottom)
                .ToList();
            var id = bhs
                .Select(x => x.Id)
                .ToList();
            var tmp = DB.Houses
                .GroupBy(x => x.BusinessHallId)
                .Select(x => new
                {
                    Key = x.Key,
                    LeftCount = x.Where(y => y.HouseStatus == HouseStatus.中国电信 && y.IsStatusChanged && (y.HardlinkStatus != Models.ServiceStatus.在用 || y.MobileStatus != Models.ServiceStatus.在用)).Count(),
                    AddedCount = x.Where(y => y.HouseStatus == HouseStatus.中国电信 && y.IsStatusChanged && (y.HardlinkStatus == Models.ServiceStatus.在用 || y.MobileStatus == Models.ServiceStatus.在用)).Count()
                })
                .ToList();
            foreach (var x in bhs)
            {
                if (!tmp.Any(a => a.Key == x.Id))
                    x.Level = 3;
                else
                {
                    var s = tmp.Where(a => a.Key == x.Id).Single();
                    x.Added = s.AddedCount;
                    x.Left = s.LeftCount;
                    if (s.LeftCount == 0)
                        x.Level = 0;
                    else if (s.LeftCount <= Convert.ToInt32(Config["Settings:Threshold:Customer:Yellow"]))
                        x.Level = 1;
                    else
                        x.Level = 2;
                    if (x.Level == 0 && s.AddedCount >= Convert.ToInt32(Config["Settings:Threshold:Customer:Cyan"]))
                        x.Level = 5;
                    if (x.Level == 1 && s.AddedCount >= Convert.ToInt32(Config["Settings:Threshold:Customer:Cyan"]))
                        x.Level = 4;
                }
            }
            return Json(bhs);
        }

        [HttpGet]
        public IActionResult Approach(string id)
        {
            var bh = DB.BusinessHalls
                .SingleOrDefault(x => x.Id == id);
            var estate = DB.Estates
                .Where(x => x.Buildings.Where(y => y.Houses.Where(z => z.BusinessHallId == id).Count() > 0).Count() > 0)
                .Select(x => new
                {
                    Lon = x.Lon,
                    Lat = x.Lat
                })
                .ToList();
            return Json(estate);
        }

        [HttpPost]
        public string ModifyBH(string id, double lon, double lat)
        {
            var bh = DB.BusinessHalls
                .Single(x => x.Id == id);
            bh.Lat = lat;
            bh.Lon = lon;
            DB.SaveChanges();
            return "ok";
        }

        [HttpGet]
        public IActionResult BusinessHallDetail(string id)
        {
            var bh = DB.BusinessHalls
               .Single(x => x.Id == id);
            var series = DB.Serieses.LastOrDefault();
            if (series != null)
            {
                var statistics = DB.Records
                    .Where(x => x.SeriesId == series.Id)
                    .Where(x => x.Status == ServiceStatus.在用)
                    .Where(x => x.BusinessHallId == id)
                    .GroupBy(x => x.Set)
                    .Select(x => new BarChartItem
                    {
                        Key = x.Key,
                        Count = x.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
                ViewBag.Statistics = statistics;
            }
            return View(bh);
        }

        [HttpGet]
        public IActionResult BusinessHallList(bool? xls, string number, string title)
        {
            IEnumerable<BusinessHall> bhs = DB.BusinessHalls;
            if (!string.IsNullOrEmpty(title))
                bhs = bhs.Where(x => x.Title.Contains(title));
            if (!string.IsNullOrEmpty(number))
                bhs = bhs.Where(x => x.Id == number);
            bhs = bhs.ToList();
            var id = bhs
                .Select(x => x.Id)
                .ToList();
            var tmp = DB.Houses
                .GroupBy(x => x.BusinessHallId)
                .Select(x => new
                {
                    Key = x.Key,
                    LeftCount = x.Where(y => y.HouseStatus == HouseStatus.中国电信 && y.IsStatusChanged && (y.HardlinkStatus != Models.ServiceStatus.在用 || y.MobileStatus != Models.ServiceStatus.在用)).Count(),
                    AddedCount = x.Where(y => y.HouseStatus == HouseStatus.中国电信 && y.IsStatusChanged && (y.HardlinkStatus == Models.ServiceStatus.在用 || y.MobileStatus == Models.ServiceStatus.在用)).Count()
                })
                .ToList();
            foreach (var x in bhs)
            {
                if (!tmp.Any(a => a.Key == x.Id))
                {
                    x.Added = 0;
                    x.Left = 0;
                }
                else
                {
                    var s = tmp.Where(a => a.Key == x.Id).Single();
                    x.Added = s.AddedCount;
                    x.Left = s.LeftCount;
                }
            }
            bhs = bhs.OrderByDescending(x => x.Added)
                .ThenBy(x => x.Left)
                .ToList();
            if (xls.HasValue && xls.Value)
                return XlsView(bhs, "businesshall.xls", "ExportBusinessHall");
            else
                return View(bhs);
        }
    }
}
