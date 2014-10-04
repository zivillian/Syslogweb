using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using SyslogWeb.Extensions;
using SyslogWeb.Models;

namespace SyslogWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index([DefaultValue(null)]string search, string date)
        {
	        DateTime parsed;

			var coll = MongoDBConfig.SyslogCollection;

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
			cursor.SetSortOrder(SortBy<SyslogEntry>.Descending(x=>x.Id));
	        var result = cursor;

			sw.Restart();
			model.LogEntries = result.Take(100).ToArray();
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

				model.QueryJson = query.ToJson(jsonSettings);
			}

	        model.RenderTime = sw;
			sw.Restart();
			return View(model);
        }
    }
}
