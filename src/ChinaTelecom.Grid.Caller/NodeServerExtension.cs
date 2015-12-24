using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Data.Entity;
using ChinaTelecom.Grid.SharedModels;
using Newtonsoft.Json;

namespace ChinaTelecom.Grid.Caller
{
    public static class NodeServerExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="timeout">unit: ms</param>
        /// <returns></returns>
        public static List<HttpClient> ToClients(this List<NodeServer> self, int? timeout = null)
        {
            var ret = new List<HttpClient>();
            foreach(var x in self)
                ret.Add(x.ToClient());
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<NodeServer> GetPrimaryNodes(this DbSet<NodeServer> self)
        {
            return self.Where(x => x.Type == ServerType.主要的 && x.Status == ServerStatus.在线)
                .ToList();
        }

        public static NodeServer GetSpecificNode(this DbSet<NodeServer> self, string city, string businessHall)
        {
            return self.Where(x => x.City == city && x.BussinessHall == businessHall && x.Type == ServerType.主要的 && x.Status == ServerStatus.在线)
                .SingleOrDefault();
        }

        public static HttpClient ToClient(this NodeServer x, int? timeout = null)
        {
            var ret = new HttpClient();
            ret.BaseAddress = new Uri($"http://{x.Server}:{x.Port}");
            ret.DefaultRequestHeaders.Accept.Clear();
            ret.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            ret.DefaultRequestHeaders.Add("private-key", x.PrivateKey);
            if (timeout.HasValue)
                ret.Timeout = new TimeSpan(0, 0, 0, 0, timeout.Value);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="timeout">unit: ms</param>
        /// <returns></returns>
        public static List<HttpClient> ToClients(this IQueryable<NodeServer> self, int? timeout = null)
        {
            return self.ToList()
                .ToClients();
        }

        public static Task GetAsync(this HttpClient x, string url, Action<HttpResponseMessage> callback = null, int? timeout = null)
        {
            return Task.Factory.StartNew(() =>
            {
                if (timeout.HasValue)
                    x.Timeout = new TimeSpan(0, 0, 0, 0, timeout.Value);
                var t = x.GetAsync(url);
                t.Wait();
                if (callback != null)
                    callback(t.Result);
            });
        }

        public static Task GetAsync(this HttpClient self, string url, Action<JsonContainer> callback = null, int? timeout = null)
        {
            if (callback == null)
                return self.GetAsync(url, null as Action<HttpResponseMessage>, timeout);
            var callback2 = new Action<HttpResponseMessage>(x =>
            {
                var t1 = x.Content.ReadAsStringAsync();
                t1.Wait();
                var t2 = JsonConvert.DeserializeObject<dynamic>(t1.Result);
                var t3 = new JsonContainer
                {
                    Status = (int)x.StatusCode,
                    Result = t2
                };
                callback(t3);
            });
            return Task.Factory.StartNew(() =>
            {
                if (timeout.HasValue)
                    self.Timeout = new TimeSpan(0, 0, 0, 0, timeout.Value);
                var t = self.GetAsync(url);
                t.Wait();
                callback2(t.Result);
            });
        }

        public static Task GetAsync(this List<HttpClient> self, string url, Action<HttpResponseMessage> callback = null, int? timeout = null)
        {
            return Task.Factory.StartNew(()=>
            {
                var tmp = new List<Task>();
                foreach (var x in self)
                    tmp.Add(x.GetAsync(url, callback, timeout));
                foreach (var x in tmp)
                    x.Wait();
            });
        }

        public static Task GetAsync(this List<HttpClient> self, string url, Action<JsonContainer> callback = null, int? timeout = null)
        {
            if (callback == null)
                return self.GetAsync(url, null as Action<HttpResponseMessage>, timeout);
            var callback2 = new Action<HttpResponseMessage>(x => 
            {
                var t1 = x.Content.ReadAsStringAsync();
                t1.Wait();
                var t2 = JsonConvert.DeserializeObject<dynamic>(t1.Result);
                var t3 = new JsonContainer
                {
                    Status = (int)x.StatusCode,
                    Result = t2
                };
                callback(t3);
            });
            return self.GetAsync(url, callback2, timeout);
        }

        public static Task PostAsync(this HttpClient x, string url, IDictionary<string, object> param = null, Action<HttpResponseMessage> callback = null, int? timeout = null)
        {
            var dic = new Dictionary<string, string>();
            if (param != null)
                foreach (var y in param)
                    dic.Add(y.Key, y.Value.ToString());
            var tmp = new List<Task>();
            return Task.Factory.StartNew(() =>
            {
                if (timeout.HasValue)
                    x.Timeout = new TimeSpan(0, 0, 0, 0, timeout.Value);
                var t = x.PostAsync(url, new FormUrlEncodedContent(dic));
                t.Wait();
                if (callback != null)
                    callback(t.Result);
            });
        }

        public static Task PostAsync(this HttpClient self, string url, IDictionary<string, object> param = null, Action<JsonContainer> callback = null, int? timeout = null)
        {
            if (callback == null)
                return self.PostAsync(url, param, null as Action<HttpResponseMessage>, timeout);
            var callback2 = new Action<HttpResponseMessage>(x =>
            {
                var t1 = x.Content.ReadAsStringAsync();
                t1.Wait();
                var t2 = JsonConvert.DeserializeObject<dynamic>(t1.Result);
                var t3 = new JsonContainer
                {
                    Status = (int)x.StatusCode,
                    Result = t2
                };
                callback(t3);
            });
            var dic = new Dictionary<string, string>();
            if (param != null)
                foreach (var y in param)
                    dic.Add(y.Key, y.Value.ToString());
            var tmp = new List<Task>();
            return Task.Factory.StartNew(() =>
            {
                if (timeout.HasValue)
                    self.Timeout = new TimeSpan(0, 0, 0, 0, timeout.Value);
                var t = self.PostAsync(url, new FormUrlEncodedContent(dic));
                t.Wait();
                callback2(t.Result);
            });
        }

        public static Task PostAsync(this List<HttpClient> self, string url, IDictionary<string, object> param = null, Action<HttpResponseMessage> callback = null, int? timeout = null)
        {
            return Task.Factory.StartNew(()=> 
            {
                var tmp = new List<Task>();
                foreach (var x in self)
                {
                    tmp.Add(x.PostAsync(url, param, callback, timeout));
                }
                foreach (var x in tmp)
                    x.Wait();
            });
        }

        public static Task PostAsync(this List<HttpClient> self, string url, IDictionary<string, object> param = null, Action<JsonContainer> callback = null, int? timeout = null)
        {
            if (callback == null)
                return self.PostAsync(url, param, null as Action<HttpResponseMessage>, timeout);
            var callback2 = new Action<HttpResponseMessage>(x =>
            {
                var t1 = x.Content.ReadAsStringAsync();
                t1.Wait();
                var t2 = JsonConvert.DeserializeObject<dynamic>(t1.Result);
                var t3 = new JsonContainer
                {
                    Status = (int)x.StatusCode,
                    Result = t2
                };
                callback(t3);
            });
            return self.PostAsync(url, param, callback2, timeout);
        }
    }
}
