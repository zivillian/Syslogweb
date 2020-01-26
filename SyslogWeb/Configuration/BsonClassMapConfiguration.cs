using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SyslogWeb.Models;

namespace SyslogWeb
{
    public static class BsonClassMapConfiguration
    {
        public static IServiceCollection ConfigureBsonMapping(this IServiceCollection services)
        {
            BsonClassMap.RegisterClassMap<SyslogEntry>(cm =>
                {
                    cm.MapProperty(c => c.Date).SetElementName("ISODATE").SetSerializer(new DateTimeOffsetSerializer());
                    cm.MapProperty(c => c.Host).SetElementName("HOST");
                    cm.MapProperty(c => c.Text).SetElementName("MESSAGE");
                    cm.MapProperty(c => c.Program).SetElementName("PROGRAM");
                    cm.MapProperty(c => c.Pid).SetElementName("PID");
                    cm.MapProperty(c => c.Seq).SetElementName("SEQNUM");
                    cm.MapProperty(c => c.Id).SetElementName("_id");
                    cm.MapProperty(c => c.Facility).SetElementName("FACILITY").SetSerializer(new FacilitySerializer());
                    cm.MapProperty(c => c.Severity).SetElementName("PRIORITY").SetSerializer(new SeveritySerializer());
                });
            return services;
        }
    }

    public class DateTimeOffsetSerializer : MongoDB.Bson.Serialization.Serializers.DateTimeOffsetSerializer
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTimeOffset value)
        {
            switch (Representation)
            {
                case BsonType.String:
                    context.Writer.WriteString(value.ToString("o"));
                    return;
                case BsonType.Array:
                    context.Writer.WriteString(value.ToString("o"));
                    return;
            }
            base.Serialize(context, args, value);
        }
    }

    public class FacilitySerializer : EnumSerializer<SyslogFacility>
    {
        public FacilitySerializer():base(BsonType.String)
        {
        }

        public override SyslogFacility Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();
            if (Enum.TryParse<SyslogFacility>(value, true, out var result))
                return result;
            return SyslogFacility.Unknown;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SyslogFacility value)
        {
            context.Writer.WriteString(value.ToString().ToLower());
        }
    }

    public class SeveritySerializer : EnumSerializer<SyslogSeverity>
    {
        public SeveritySerializer():base(BsonType.String)
        {
        }

        public override SyslogSeverity Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();
            if (Enum.TryParse<SyslogSeverity>(value, true, out var result))
                return result;
            switch (value)
            {
                case "info":
                    return SyslogSeverity.Informational;
                case "err":
                    return SyslogSeverity.Error;
                case "crit":
                    return SyslogSeverity.Critical;
                case "emerg":
                    return SyslogSeverity.Emergency;
                default:
                    return SyslogSeverity.Unknown;
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SyslogSeverity value)
        {
            switch (value)
            {
                case SyslogSeverity.Informational:
                    context.Writer.WriteString("info");
                    break;
                case SyslogSeverity.Emergency:
                    context.Writer.WriteString("emerg");
                    break;
                default:
                    context.Writer.WriteString(value.ToString().ToLower());
                    break;
            }
        }
    }
}