using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SyslogWeb.Models;

namespace SyslogWeb
{
	public class ClassMap
	{
		public static void Register()
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

	public class FacilitySerializer : IBsonSerializer<SyslogFacility>
	{
		public SyslogFacility Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var value = context.Reader.ReadString();
			switch (value)
			{
				case "auth":
					return SyslogFacility.auth;
				case "authpriv":
					return SyslogFacility.authpriv;
				case "cron":
					return SyslogFacility.cron;
				case "daemon":
					return SyslogFacility.daemon;
				case "ftp":
					return SyslogFacility.ftp;
				case "kern":
					return SyslogFacility.kern;
				case "local0":
					return SyslogFacility.local0;
				case "local1":
					return SyslogFacility.local1;
				case "local2":
					return SyslogFacility.local2;
				case "local3":
					return SyslogFacility.local3;
				case "local4":
					return SyslogFacility.local4;
				case "local5":
					return SyslogFacility.local5;
				case "local6":
					return SyslogFacility.local6;
				case "local7":
					return SyslogFacility.local7;
				case "lpr":
					return SyslogFacility.lpr;
				case "mail":
					return SyslogFacility.mail;
				case "news":
					return SyslogFacility.news;
				case "syslog":
					return SyslogFacility.syslog;
				case "user":
					return SyslogFacility.user;
				case "uucp":
					return SyslogFacility.uucp;
				case "ntp":
					return SyslogFacility.ntp;
				default:
					throw new NotSupportedException(value);
			}
		}

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SyslogFacility value)
		{
			switch (value)
			{
				case SyslogFacility.kern:
					context.Writer.WriteString("kern");
					break;
				case SyslogFacility.user:
					context.Writer.WriteString("user");
					break;
				case SyslogFacility.mail:
					context.Writer.WriteString("mail");
					break;
				case SyslogFacility.daemon:
					context.Writer.WriteString("daemon");
					break;
				case SyslogFacility.auth:
					context.Writer.WriteString("auth");
					break;
				case SyslogFacility.syslog:
					context.Writer.WriteString("syslog");
					break;
				case SyslogFacility.lpr:
					context.Writer.WriteString("lpr");
					break;
				case SyslogFacility.news:
					context.Writer.WriteString("news");
					break;
				case SyslogFacility.uucp:
					context.Writer.WriteString("uucp");
					break;
				case SyslogFacility.authpriv:
					context.Writer.WriteString("authpriv");
					break;
				case SyslogFacility.ftp:
					context.Writer.WriteString("ftp");
					break;
				case SyslogFacility.cron:
					context.Writer.WriteString("cron");
					break;
				case SyslogFacility.local0:
					context.Writer.WriteString("local0");
					break;
				case SyslogFacility.local1:
					context.Writer.WriteString("local1");
					break;
				case SyslogFacility.local2:
					context.Writer.WriteString("local2");
					break;
				case SyslogFacility.local3:
					context.Writer.WriteString("local3");
					break;
				case SyslogFacility.local4:
					context.Writer.WriteString("local4");
					break;
				case SyslogFacility.local5:
					context.Writer.WriteString("local5");
					break;
				case SyslogFacility.local6:
					context.Writer.WriteString("local6");
					break;
				case SyslogFacility.local7:
					context.Writer.WriteString("local7");
					break;
				case SyslogFacility.ntp:
					context.Writer.WriteString("ntp");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var facility = value is SyslogFacility ? (SyslogFacility)value : SyslogFacility.Unknown;
			Serialize(context, args, facility);
        }

        public Type ValueType => typeof(SyslogFacility);
    }

	public class SeveritySerializer : IBsonSerializer<SyslogSeverity>
	{
		public SyslogSeverity Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var value = context.Reader.ReadString();
			switch (value)
			{
				case "info":
					return SyslogSeverity.Informational;
				case "err":
				case "error":
					return SyslogSeverity.Error;
				case "notice":
					return SyslogSeverity.Notice;
				case "critical":
				case "crit":
					return SyslogSeverity.Critical;
				case "debug":
					return SyslogSeverity.Debug;
				case "warning":
					return SyslogSeverity.Warning;
				case "emerg":
					return SyslogSeverity.Emergency;
				case "alert":
					return SyslogSeverity.Alert;
				default:
					throw new NotSupportedException(value);
			}
		}

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var severity = value is SyslogSeverity ? (SyslogSeverity) value : SyslogSeverity.Unknown;
			Serialize(context, args, severity);
        }

		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SyslogSeverity value)
		{
			switch (value)
			{
				case SyslogSeverity.Critical:
					context.Writer.WriteString("critical");
					break;
				case SyslogSeverity.Crit:
					context.Writer.WriteString("crit");
					break;
				case SyslogSeverity.Error:
					context.Writer.WriteString("error");
					break;
				case SyslogSeverity.Err:
					context.Writer.WriteString("err");
					break;
				case SyslogSeverity.Notice:
					context.Writer.WriteString("notice");
					break;
				case SyslogSeverity.Informational:
					context.Writer.WriteString("info");
					break;
				case SyslogSeverity.Debug:
					context.Writer.WriteString("debug");
					break;
				case SyslogSeverity.Warning:
					context.Writer.WriteString("warning");
					break;
				case SyslogSeverity.Emergency:
					context.Writer.WriteString("emerg");
					break;
				case SyslogSeverity.Alert:
					context.Writer.WriteString("alert");
					break;
				case SyslogSeverity.Unknown:
					throw new ArgumentOutOfRangeException();
			}
		}
        public Type ValueType => typeof(SyslogSeverity);
	}
}