using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Wrappers;
using SyslogWeb.Models;

namespace SyslogWeb
{
	public class MongoDBConfig
	{
		public static MongoClient Client { get; private set; }

		public static MongoServer Server{get { return Client.GetServer(); }}

		public static MongoDatabase SyslogDB { get { return Server.GetDatabase(Properties.Settings.Default.MongoDBName); } }
		
		public static MongoCollection<SyslogEntry> SyslogCollection { get { return SyslogDB.GetCollection<SyslogEntry>(Properties.Settings.Default.MongoDBCollection); } }

		public static void Register()
		{
			var connectionString = Properties.Settings.Default.MongoDB;
#if DEBUG
			if (Environment.MachineName.ToLowerInvariant() == "blackbox")
				connectionString = "mongodb://dana.bln.wg1337.de/";
#endif
			Client = new MongoClient(connectionString);
			CreateIndices();
		}

		private static readonly Dictionary<string, IMongoIndexKeys> _indices = new Dictionary<string, IMongoIndexKeys>
			{
				{"program__id", new IndexKeysBuilder<SyslogEntry>().Ascending(x=>x.Program).Descending(x=>x.Id)},
				{"host__id", new IndexKeysBuilder<SyslogEntry>().Ascending(x=>x.Host).Descending(x=>x.Id)},
				{"priority__id", new IndexKeysBuilder<SyslogEntry>().Ascending(x=>x.Severity).Descending(x=>x.Id)},
				{"facility__id", new IndexKeysBuilder<SyslogEntry>().Ascending(x=>x.Facility).Descending(x=>x.Id)},
				{"isodate__id", new IndexKeysBuilder<SyslogEntry>().Ascending(x=>x.Date).Descending(x=>x.Id)},
				{"program_host_priority_facility_isodate__id", new IndexKeysBuilder<SyslogEntry>().Ascending(x=>x.Program).Ascending(x=>x.Host).Ascending(x=>x.Severity).Ascending(x=>x.Facility).Ascending(x=>x.Date).Descending(x=>x.Id)},
				{"_fts__ftsx", new IndexKeysBuilder().Text("MESSAGE", "PROGRAM", "PRIORITY", "HOST", "FACILITY").Text()},
			};

		private static void CreateIndices()
		{
			var existing = SyslogCollection.GetIndexes()
				.Select(index => String.Join("_", index.Key.Names.Select(x => x.ToLowerInvariant())))
				.ToArray();
			var missing = _indices.Where(x => !existing.Contains(x.Key)).Select(x => x.Value).ToArray();
			foreach (var index in missing)
			{
				SyslogCollection.CreateIndex(index);
			}
		}
	}
}