using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Entity;
using Newtonsoft.Json;
using ChinaTelecom.OperateBrace.Models;
using ChinaTelecom.OperateBrace.Lib;

namespace ChinaTelecom.OperateBrace.Controllers
{
    [Authorize]
    public class RecordController : BaseController
    {
        [FromServices]
        public IServiceProvider services { get; set; }

        public IActionResult Index(string ContractorName, string StaffName, ServiceStatus? Status, string Name, string Address, string Account, string Set, string Phone, string xls, Guid? SeriesId, DateTime? BeginTime, DateTime? EndTime)
        {
            IEnumerable<Record> ret = DB.Records.AsNoTracking();
            if (Status.HasValue)
                ret = ret.Where(x => x.Status == Status);
            if (!string.IsNullOrEmpty(Set))
                ret = ret.Where(x => x.Set == Set);
            if (!string.IsNullOrEmpty(Address))
                ret = ret.Where(x => x.ImplementAddress.Contains(Address) || x.StandardAddress.Contains(Address));
            if (User.IsInRole("网格经理"))
            {
                ret = ret.Where(x => x.ContractorName == User.Current.FullName || x.ServiceStaff == User.Current.FullName);
            }
            else
            {
                if (!string.IsNullOrEmpty(ContractorName))
                    ret = ret.Where(x => x.ContractorName == ContractorName);
                if (!string.IsNullOrEmpty(StaffName))
                    ret = ret.Where(x => x.ServiceStaff == StaffName);
            }
            if (!string.IsNullOrEmpty(Name))
                ret = ret.Where(x => x.CustomerName == Name);
            if (!string.IsNullOrEmpty(Phone))
                ret = ret.Where(x => x.Phone.Contains(Phone));
            if (!string.IsNullOrEmpty(Account))
                ret = ret.Where(x => x.Account == Account);
            if (SeriesId.HasValue)
                ret = ret.Where(x => x.SeriesId == SeriesId.Value);
            if (BeginTime.HasValue)
                ret = ret.Where(x => x.ImportedTime >= BeginTime.Value);
            if (EndTime.HasValue)
            {
                EndTime = EndTime.Value.AddDays(1);
                ret = ret.Where(x => x.ImportedTime < EndTime.Value);
            }
            ret = ret.OrderByDescending(x => x.ImportedTime);
            if (xls != "true")
            {
                ViewBag.Statuses = DB.Records
                    .Select(x => x.Status.ToString())
                    .Distinct()
                    .ToList();
                ViewBag.Sets = DB.Records
                    .Select(x => x.Set)
                    .Distinct()
                    .ToList();
                if (User.IsInRole("网格经理"))
                {
                    ViewBag.Staff = new List<string> { User.Current.FullName };
                    ViewBag.ContractorNames = new List<string> { User.Current.FullName };
                }
                else
                {
                    ViewBag.ContractorNames = DB.Records
                        .Select(x => x.ContractorName)
                        .Distinct()
                        .ToList();
                    ViewBag.Staff = DB.Records
                    .Select(x => x.ServiceStaff)
                    .Distinct()
                    .ToList();
                }
                ViewBag.RecordCount = ret.Count();
                return PagedView(ret);
            }
            else
            {
                return XlsView(ret.ToList(), "chinatelecom.xls", "Xls");
            }
        }

        [AnyRoles("系统管理员, 网格主管")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import(IFormFile file, [FromServices] IConfiguration Config)
        {
            // 创建明细系列
            var series = new Series
            {
                Time = DateTime.Now
            };
            DB.Serieses.Add(series);
            DB.SaveChanges();

            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            Task.Factory.StartNew(() =>
            {
                using (var serviceScope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    DB = serviceScope.ServiceProvider.GetService<GridContext>();
                    DB.ChangeTracker.AutoDetectChangesEnabled = false;
                    DB.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                    series = DB.Serieses.Where(x => x.Id == series.Id).Single();
                    using (var conn = new OleDbConnection(connStr))
                    {
                        conn.Open();
                        var schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                        var rows = schemaTable.Rows;
                        foreach (DataRow r in rows)
                        {
                            if (r["TABLE_NAME"].ToString().Last() != '$')
                                continue;
                            using (var adapter = new OleDbDataAdapter($"select * from [{r["TABLE_NAME"].ToString()}]", conn))
                            using (var dt = new DataTable())
                            {
                                adapter.Fill(dt);
                                lock (this)
                                {
                                    series = DB.Serieses.Where(x => x.Id == series.Id).Single();
                                    series.TotalCount = series.TotalCount + dt.Rows.Count;
                                    DB.Update(series);
                                    DB.SaveChanges();
                                }

                                for (var i = 0; i < dt.Rows.Count; i++)
                                {
                                    var reader = dt.Rows[i];
                                    using (var innerServiceScope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                                    using (var db = innerServiceScope.ServiceProvider.GetService<GridContext>())
                                    {
                                        db.ChangeTracker.AutoDetectChangesEnabled = false;
                                        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                                        try
                                        {
                                            var record = new Record
                                            {
                                                Account = reader["接入号"].ToString(),
                                                CustomerName = reader["用户姓名"].ToString(),
                                                ContractorName = reader["四级承包人名称"].ToString(),
                                                ContractorStruct = reader["四级承包体名称"].ToString(),
                                                ServiceStaff = reader["包服人"].ToString(),
                                                MDS = reader["中投"].ToString(),
                                                ImplementAddress = reader["装机地址"].ToString(),
                                                StandardAddress = reader["标准地址"].ToString(),
                                                Set = reader["套餐"].ToString(),
                                                SalesProduction = reader["融合促销包"].ToString(),
                                                Phone = reader["联系电话"].ToString(),
                                                IsFuse = reader["是否家庭融合宽带"].ToString() == "是",
                                                ImportedTime = series.Time,
                                                SeriesId = series.Id,
                                                BusinessHallId = reader["营业厅编号"].ToString(),
                                                BusinessHallName = reader["营业厅名称"].ToString(),
                                                FuseIdentifier = reader["用户标识"].ToString(),
                                                IsHardLink = reader["用户类型"].ToString() != "CDMA"
                                            };
                                            #region Try parse
                                            try
                                            {
                                                record.CurrentMonthBill = Convert.ToDouble(reader["当月出帐"].ToString());
                                            }
                                            catch
                                            {
                                                record.CurrentMonthBill = 0;
                                            }

                                            try
                                            {
                                                record.AgentFee = Convert.ToDouble(reader["代理费"].ToString());
                                            }
                                            catch
                                            {
                                                record.AgentFee = 0;
                                            }

                                            try
                                            {
                                                record.Commission = Convert.ToDouble(reader["一次佣金"].ToString());
                                            }
                                            catch
                                            {
                                                record.Commission = 0;
                                            }

                                            try
                                            {
                                                record.Arrearage = Convert.ToDouble(reader["欠费"].ToString());
                                            }
                                            catch
                                            {
                                                record.Arrearage = 0;
                                            }

                                            try
                                            {
                                                if (reader["用户状态"].ToString() == "欠停(双向)")
                                                    record.Status = ServiceStatus.双向欠停;
                                                else if (reader["用户状态"].ToString() == "欠停(单向)")
                                                    record.Status = ServiceStatus.单向欠停;
                                                else
                                                    record.Status = (ServiceStatus)Enum.Parse(typeof(ServiceStatus), reader["用户状态"].ToString());
                                            }
                                            catch
                                            {
                                                record.Status = ServiceStatus.未知;
                                            }

                                            try
                                            {
                                                record.PRCID = (Convert.ToInt64(reader["身份证号码"])).ToString();
                                            }
                                            catch
                                            {
                                                record.PRCID = null;
                                            }
                                            #endregion
                                            db.Records.Add(record);
                                            db.SaveChanges();
                                            // 如果记录是固网的，才有可能进行营业厅和具体楼宇的对应
                                            if (record.IsHardLink)
                                            {
                                                #region Creating business hall
                                                var bh = db.BusinessHalls
                                                    .Where(x => x.Id == record.BusinessHallId)
                                                    .SingleOrDefault();
                                                try
                                                {
                                                    if (bh == null)
                                                    {
                                                        bh = new BusinessHall
                                                        {
                                                            Id = record.BusinessHallId,
                                                            Title = Lib.AddressAnalyser.FilterBrackets(record.BusinessHallName)
                                                        };
                                                        var title = bh.Title;
                                                        if (bh.Title.IndexOf("中国电信") < 0)
                                                            title = "中国电信" + bh.Title;
                                                        if (bh.Title.IndexOf("营业厅") < 0)
                                                            title = bh.Title + "营业厅";
                                                        var bmapJson = Lib.HttpHelper.Get($"http://api.map.baidu.com/geocoder/v2/?city={Config["BMap:City"]}&address={title}&output=json&ak={Config["BMap:ApplicationKey"]}");
                                                        dynamic bmap = JsonConvert.DeserializeObject<dynamic>(bmapJson);
                                                        bh.Lon = bmap.result.location.lng;
                                                        bh.Lat = bmap.result.location.lat;
                                                        db.BusinessHalls.Add(bh);
                                                        db.SaveChanges();
                                                    }
                                                }
                                                catch
                                                {
                                                }
                                                #endregion
                                                #region Mapping to house
                                                try
                                                {
                                                    // 查找是否有对应到楼宇的记录
                                                    var house = db.Houses
                                                        .Include(x => x.Building)
                                                        .ThenInclude(x => x.Estate)
                                                        .ThenInclude(x => x.Rules)
                                                        .Where(x => x.Account == record.Account)
                                                        .SingleOrDefault();
                                                    if (house != null)
                                                    {
                                                        // 检查地址变更
                                                        var rules = house.Building.Estate.Rules
                                                            .Select(x => x.Rule)
                                                            .ToList();
                                                        var flag = false;
                                                        foreach (var x in rules)
                                                        {
                                                            if (record.ImplementAddress.Contains(x) || record.StandardAddress.Contains(x))
                                                            {
                                                                flag = true;
                                                                break;
                                                            }
                                                        }

                                                        // 如果地址变更则更新地址信息
                                                        if (!flag)
                                                        {
                                                            var estate = db.EstateRules
                                                                .Include(x => x.Estate)
                                                                .ThenInclude(x => x.Buildings)
                                                                .Where(x => record.StandardAddress.Contains(x.Rule) || record.ImplementAddress.Contains(x.Rule))
                                                                .Select(x => x.Estate)
                                                                .FirstOrDefault();
                                                            if (estate != null)
                                                            {
                                                                var building = Lib.AddressAnalyser.GetBuildingNumber(record.ImplementAddress);
                                                                var _building = estate.Buildings
                                                                    .Where(a => a.Title == building)
                                                                    .SingleOrDefault();
                                                                var unit = Lib.AddressAnalyser.GetUnit(record.ImplementAddress);
                                                                var layer = Lib.AddressAnalyser.GetLayer(record.ImplementAddress);
                                                                var door = Lib.AddressAnalyser.GetDoor(record.ImplementAddress);
                                                                if (!string.IsNullOrEmpty(building) && unit.HasValue && layer.HasValue && door.HasValue && _building != null)
                                                                {
                                                                    if (db.Houses.Where(x => x.BuildingId == _building.Id && x.Unit == unit.Value && x.Layer == layer.Value && x.Door == door.Value).Count() == 0)
                                                                    {
                                                                        if (unit.Value > _building.Units)
                                                                            _building.Units = unit.Value;
                                                                        if (layer.Value < _building.BottomLayers)
                                                                            _building.BottomLayers = layer.Value;
                                                                        if (layer.Value > _building.TopLayers)
                                                                            _building.TopLayers = layer.Value;
                                                                        if (door > _building.Doors)
                                                                            _building.Doors = door.Value;
                                                                        house.BuildingId = _building.Id;
                                                                        house.Unit = unit.Value;
                                                                        house.Layer = layer.Value;
                                                                        house.Door = door.Value;
                                                                        house.Phone = record.Phone;
                                                                        house.FullName = record.CustomerName;
                                                                        house.IsStatusChanged = record.Status != house.ServiceStatus;
                                                                        house.HardlinkStatus = record.Status;
                                                                        house.FuseIdentifier = record.FuseIdentifier;
                                                                        house.IsFuse = record.IsFuse;
                                                                        house.LastUpdate = DateTime.Now;
                                                                        house.HouseStatus = HouseStatus.中国电信;
                                                                        house.BusinessHallId = bh.Id;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var estate = db.EstateRules
                                                                 .Include(x => x.Estate)
                                                                 .ThenInclude(x => x.Buildings)
                                                                 .Where(x => record.StandardAddress.Contains(x.Rule) || record.ImplementAddress.Contains(x.Rule))
                                                                 .Select(x => x.Estate)
                                                                 .FirstOrDefault();
                                                        if (estate != null)
                                                        {
                                                            var building = Lib.AddressAnalyser.GetBuildingNumber(record.ImplementAddress);
                                                            var _building = db.Buildings
                                                                .Where(a => a.EstateId == estate.Id && a.Title == building)
                                                                .SingleOrDefault();
                                                            var unit = Lib.AddressAnalyser.GetUnit(record.ImplementAddress);
                                                            var layer = Lib.AddressAnalyser.GetLayer(record.ImplementAddress);
                                                            var door = Lib.AddressAnalyser.GetDoor(record.ImplementAddress);
                                                            if (!string.IsNullOrEmpty(building) && unit.HasValue && layer.HasValue && door.HasValue)
                                                            {
                                                                if (_building != null)
                                                                {
                                                                    if (db.Houses.Where(x => x.BuildingId == _building.Id && x.Unit == unit.Value && x.Layer == layer.Value && x.Door == door.Value).Count() == 0)
                                                                    {
                                                                        if (unit.Value > _building.Units)
                                                                            _building.Units = unit.Value;
                                                                        if (layer.Value < _building.BottomLayers)
                                                                            _building.BottomLayers = layer.Value;
                                                                        if (layer.Value > _building.TopLayers)
                                                                            _building.TopLayers = layer.Value;
                                                                        if (door > _building.Doors)
                                                                            _building.Doors = door.Value;
                                                                        db.Update(_building);
                                                                        db.SaveChanges();
                                                                        house = new House();
                                                                        house.Account = record.Account;
                                                                        house.BuildingId = _building.Id;
                                                                        house.Unit = unit.Value;
                                                                        house.Layer = layer.Value;
                                                                        house.Door = door.Value;
                                                                        house.Phone = record.Phone;
                                                                        house.FullName = record.CustomerName;
                                                                        house.IsStatusChanged = true;
                                                                        house.HardlinkStatus = record.Status;
                                                                        house.FuseIdentifier = record.FuseIdentifier;
                                                                        house.IsFuse = record.IsFuse;
                                                                        house.LastUpdate = DateTime.Now;
                                                                        house.HouseStatus = HouseStatus.中国电信;
                                                                        house.BusinessHallId = bh.Id;
                                                                        db.Houses.Add(house);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (door < 2)
                                                                        door = 2;
                                                                    int bottom = 1;
                                                                    int top = 1;
                                                                    if (layer < 0)
                                                                        bottom = layer.Value;
                                                                    if (layer > 0)
                                                                        top = layer.Value;
                                                                    _building = new Building
                                                                    {
                                                                        Title = building,
                                                                        EstateId = estate.Id,
                                                                        BottomLayers = bottom,
                                                                        TopLayers = top,
                                                                        Units = unit.Value,
                                                                        Doors = door.Value
                                                                    };
                                                                    db.Buildings.Add(_building);
                                                                    house = new House();
                                                                    house.Account = record.Account;
                                                                    house.BuildingId = _building.Id;
                                                                    house.Unit = unit.Value;
                                                                    house.Layer = layer.Value;
                                                                    house.Door = door.Value;
                                                                    house.Phone = record.Phone;
                                                                    house.FullName = record.CustomerName;
                                                                    house.IsStatusChanged = true;
                                                                    house.HardlinkStatus = record.Status;
                                                                    house.IsFuse = record.IsFuse;
                                                                    house.FuseIdentifier = record.FuseIdentifier;
                                                                    house.LastUpdate = DateTime.Now;
                                                                    house.HouseStatus = HouseStatus.中国电信;
                                                                    house.BusinessHallId = bh.Id;
                                                                    db.Houses.Add(house);
                                                                }
                                                            }
                                                        }
                                                        else // 不存在则尝试创建
                                                        {
                                                            var estateTitle = Lib.AddressAnalyser.GetEstate(record.ImplementAddress);
                                                            if (!string.IsNullOrEmpty(estateTitle))
                                                            {
                                                                var bmapJson = Lib.HttpHelper.Get($"http://api.map.baidu.com/geocoder/v2/?city={Config["BMap:City"]}&address={record.ImplementAddress}&output=json&ak={Config["BMap:ApplicationKey"]}");
                                                                dynamic bmap = JsonConvert.DeserializeObject<dynamic>(bmapJson);
                                                                if (bmap.status == 0)
                                                                {
                                                                    var es = new Estate
                                                                    {
                                                                        Title = estateTitle,
                                                                        Lon = bmap.result.location.lng,
                                                                        Lat = bmap.result.location.lat,
                                                                        Area = ""
                                                                    };
                                                                    db.Estates.Add(es);
                                                                    db.EstateRules.Add(new EstateRule
                                                                    {
                                                                        EstateId = es.Id,
                                                                        Rule = estateTitle
                                                                    });
                                                                    var building = Lib.AddressAnalyser.GetBuildingNumber(record.ImplementAddress);
                                                                    var unit = Lib.AddressAnalyser.GetUnit(record.ImplementAddress);
                                                                    var layer = Lib.AddressAnalyser.GetLayer(record.ImplementAddress);
                                                                    var door = Lib.AddressAnalyser.GetDoor(record.ImplementAddress);
                                                                    if (!string.IsNullOrEmpty(building) && unit.HasValue && layer.HasValue && door.HasValue)
                                                                    {
                                                                        if (door < 2)
                                                                            door = 2;
                                                                        int bottom = 1;
                                                                        int top = 1;
                                                                        if (layer < 0)
                                                                            bottom = layer.Value;
                                                                        if (layer > 0)
                                                                            top = layer.Value;
                                                                        var _building = new Building
                                                                        {
                                                                            Title = building,
                                                                            EstateId = es.Id,
                                                                            BottomLayers = bottom,
                                                                            TopLayers = top,
                                                                            Units = unit.Value,
                                                                            Doors = door.Value
                                                                        };
                                                                        db.Buildings.Add(_building);
                                                                        house = new House();
                                                                        house.Account = record.Account;
                                                                        house.BuildingId = _building.Id;
                                                                        house.Unit = unit.Value;
                                                                        house.Layer = layer.Value;
                                                                        house.Door = door.Value;
                                                                        house.Phone = record.Phone;
                                                                        house.FullName = record.CustomerName;
                                                                        house.IsStatusChanged = true;
                                                                        house.HardlinkStatus = record.Status;
                                                                        house.FuseIdentifier = record.FuseIdentifier;
                                                                        house.LastUpdate = DateTime.Now;
                                                                        house.HouseStatus = HouseStatus.中国电信;
                                                                        house.BusinessHallId = bh.Id;
                                                                        db.Houses.Add(house);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    db.ChangeTracker.DetectChanges();
                                                    db.SaveChanges();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex.ToString());
                                                }
                                                GC.Collect();
                                                #endregion
                                            }
                                            else // 否则只能根据用户标识来查询楼宇
                                            {
                                                var house = db.Houses
                                                    .Where(x => x.FuseIdentifier == record.FuseIdentifier)
                                                    .FirstOrDefault();
                                                if (house != null)
                                                {
                                                    // 如果找到了用户标识对应的house，则更新移动网状态
                                                    house.MobileStatus = record.Status;
                                                    house.IsStatusChanged = record.Status != house.ServiceStatus;
                                                    house.IsFuse = true;
                                                    house.LastUpdate = DateTime.Now;
                                                    db.Update(house);
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            series = db.Serieses.Where(x => x.Id == series.Id).Single();
                                            series.FailedCount++;
                                            db.Update(series);
                                            db.SaveChanges();
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            });
            return RedirectToAction("Series", "Record");
        }

        public IActionResult Series()
        {
            var ret = DB.Serieses
                .OrderByDescending(x => x.Time);
            return PagedView(ret);
        }

        [AnyRoles("系统管理员, 网格主管")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSeries(Guid id)
        {
            var series = DB.Serieses
                .Where(x => x.Id == id)
                .Single();
            DB.Serieses.Remove(series);
            DB.SaveChanges();
            return RedirectToAction("Series", "Record");
        }
    }
}