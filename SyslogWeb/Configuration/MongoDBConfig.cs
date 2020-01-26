using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using SyslogWeb.Models;

namespace SyslogWeb
{
    public class MongoDBConfig
    {
        private readonly MongoDBOptions _options;

        public MongoDBConfig(IOptions<MongoDBOptions> options)
        :this(options.Value)
        {}
        public MongoDBConfig(MongoDBOptions options)
        {
            _options = options;
            var settings = MongoClientSettings.FromConnectionString(_options.ConnectionString);
            settings.ReadEncoding = Utf8Encodings.Lenient;
            Client = new MongoClient(settings);
            CreateIndices();
        }

        public MongoClient Client { get; private set; }
        
        public IMongoDatabase SyslogDB { get { return Client.GetDatabase(_options.Database); } }
        
        public IMongoCollection<SyslogEntry> SyslogCollection { get { return SyslogDB.GetCollection<SyslogEntry>(_options.Collection); } }
        
        private static readonly Dictionary<string, IndexKeysDefinition<SyslogEntry>> _indices = new Dictionary<string, IndexKeysDefinition<SyslogEntry>>
            {
                {"program__id", new IndexKeysDefinitionBuilder<SyslogEntry>().Ascending(x=>x.Program).Descending(x=>x.Id)},
                {"host__id", new IndexKeysDefinitionBuilder<SyslogEntry>().Ascending(x=>x.Host).Descending(x=>x.Id)},
                {"priority__id", new IndexKeysDefinitionBuilder<SyslogEntry>().Ascending(x=>x.Severity).Descending(x=>x.Id)},
                {"facility__id", new IndexKeysDefinitionBuilder<SyslogEntry>().Ascending(x=>x.Facility).Descending(x=>x.Id)},
                {"isodate__id", new IndexKeysDefinitionBuilder<SyslogEntry>().Ascending(x=>x.Date).Descending(x=>x.Id)},
                {"program_host_priority_facility_isodate__id", new IndexKeysDefinitionBuilder<SyslogEntry>().Ascending(x=>x.Program).Ascending(x=>x.Host).Ascending(x=>x.Severity).Ascending(x=>x.Facility).Ascending(x=>x.Date).Descending(x=>x.Id)},
                {"_fts__ftsx", new IndexKeysDefinitionBuilder<SyslogEntry>().Text(x=>x.Text).Text(x=>x.Program).Text(x=>x.Severity).Text(x=>x.Host).Text(x=>x.Facility)},
            };

        private void CreateIndices()
        {
            var existing = SyslogCollection.Indexes.List().ToEnumerable()
                .Select(index => String.Join("_", index.Names.Select(x => x.ToLowerInvariant())))
                .ToArray();
            var missing = _indices.Where(x => !existing.Contains(x.Key)).Select(x => x.Value).ToArray();
            foreach (var index in missing)
            {
                SyslogCollection.Indexes.CreateOne(index);
            }
        }
    }
}