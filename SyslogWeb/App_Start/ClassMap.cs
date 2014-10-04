using System;
using System.Xml;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
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
		public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value,
		                               IBsonSerializationOptions options)
		{
			var dateTimeOffset = (DateTimeOffset)value;
			var representationSerializationOption = EnsureSerializationOptions<RepresentationSerializationOptions>(options);
			if (value != null)
			switch (representationSerializationOption.Representation)
			{
				case BsonType.String:
					bsonWriter.WriteString(dateTimeOffset.ToString("o"));
					return;
				case BsonType.Array:
					bsonWriter.WriteString(dateTimeOffset.ToString("o"));
					return;
			}
			base.Serialize(bsonWriter, nominalType, value, options);
		}
	}

	public class FacilitySerializer : IBsonSerializer
	{
		public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			var value = bsonReader.ReadString();
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

		public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			var facility = value is SyslogFacility ? (SyslogFacility)value : SyslogFacility.Unknown;
			switch (facility)
			{
				case SyslogFacility.kern:
					bsonWriter.WriteString("kern");
					break;
				case SyslogFacility.user:
					bsonWriter.WriteString("user");
					break;
				case SyslogFacility.mail:
					bsonWriter.WriteString("mail");
					break;
				case SyslogFacility.daemon:
					bsonWriter.WriteString("daemon");
					break;
				case SyslogFacility.auth:
					bsonWriter.WriteString("auth");
					break;
				case SyslogFacility.syslog:
					bsonWriter.WriteString("syslog");
					break;
				case SyslogFacility.lpr:
					bsonWriter.WriteString("lpr");
					break;
				case SyslogFacility.news:
					bsonWriter.WriteString("news");
					break;
				case SyslogFacility.uucp:
					bsonWriter.WriteString("uucp");
					break;
				case SyslogFacility.authpriv:
					bsonWriter.WriteString("authpriv");
					break;
				case SyslogFacility.ftp:
					bsonWriter.WriteString("ftp");
					break;
				case SyslogFacility.cron:
					bsonWriter.WriteString("cron");
					break;
				case SyslogFacility.local0:
					bsonWriter.WriteString("local0");
					break;
				case SyslogFacility.local1:
					bsonWriter.WriteString("local1");
					break;
				case SyslogFacility.local2:
					bsonWriter.WriteString("local2");
					break;
				case SyslogFacility.local3:
					bsonWriter.WriteString("local3");
					break;
				case SyslogFacility.local4:
					bsonWriter.WriteString("local4");
					break;
				case SyslogFacility.local5:
					bsonWriter.WriteString("local5");
					break;
				case SyslogFacility.local6:
					bsonWriter.WriteString("local6");
					break;
				case SyslogFacility.local7:
					bsonWriter.WriteString("local7");
					break;
				case SyslogFacility.ntp:
					bsonWriter.WriteString("ntp");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			return Deserialize(bsonReader, nominalType, options);
		}

		public IBsonSerializationOptions GetDefaultSerializationOptions()
		{
			return null;
		}
	}

	public class SeveritySerializer : IBsonSerializer
	{
		public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
		{
			var value = bsonReader.ReadString();
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

		public object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			return Deserialize(bsonReader, nominalType, options);
		}

		public IBsonSerializationOptions GetDefaultSerializationOptions()
		{
			return null;
		}

		public void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
		{
			var severity = value is SyslogSeverity ? (SyslogSeverity) value : SyslogSeverity.Unknown;
			switch (severity)
			{
				case SyslogSeverity.Critical:
					bsonWriter.WriteString("critical");
					break;
				case SyslogSeverity.Crit:
					bsonWriter.WriteString("crit");
					break;
				case SyslogSeverity.Error:
					bsonWriter.WriteString("error");
					break;
				case SyslogSeverity.Err:
					bsonWriter.WriteString("err");
					break;
				case SyslogSeverity.Notice:
					bsonWriter.WriteString("notice");
					break;
				case SyslogSeverity.Informational:
					bsonWriter.WriteString("info");
					break;
				case SyslogSeverity.Debug:
					bsonWriter.WriteString("debug");
					break;
				case SyslogSeverity.Warning:
					bsonWriter.WriteString("warning");
					break;
				case SyslogSeverity.Emergency:
					bsonWriter.WriteString("emerg");
					break;
				case SyslogSeverity.Alert:
					bsonWriter.WriteString("alert");
					break;
				case SyslogSeverity.Unknown:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}