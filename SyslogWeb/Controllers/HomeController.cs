using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SyslogWeb.Models;

namespace SyslogWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly MongoDBConfig _mongoDb;

        public HomeController(MongoDBConfig mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public ActionResult Index([DefaultValue(null)]string search, string date)
        {
	        DateTime parsed;

			var coll = _mongoDb.SyslogCollection;

			var model = new MongoResultModel();
			if (DateTime.TryParse(date, out parsed))
			{
				model.Date = parsed;
			}
			var sw = new Stopwatch();
			sw.Start();
			var query = QueryParser.Parse(search, model);
			sw.Stop();
	        model.ParseTime = sw.ElapsedMilliseconds;
			var cursor = coll.Find(query);
			cursor.Sort(new SortDefinitionBuilder<SyslogEntry>().Descending(x=>x.Id));
	        var result = cursor;

			sw.Restart();
			model.LogEntries = result.Limit(100).ToList();
			sw.Stop();
			model.FetchTime = sw.ElapsedMilliseconds;

			sw.Restart();
			model.Total = result.Count();
			sw.Stop();
			model.CountTime = sw.ElapsedMilliseconds;
			
			if (query != null)
			{
				var jsonSettings = new JsonWriterSettings
				{
					Indent = true,
					OutputMode = JsonOutputMode.Strict
				};

				model.QueryJson = query.Render(BsonSerializer.SerializerRegistry.GetSerializer<SyslogEntry>(), BsonSerializer.SerializerRegistry).ToJson(jsonSettings);
			}

	        model.RenderTime = sw;
			sw.Restart();
			return View(model);
        }
    }
}
