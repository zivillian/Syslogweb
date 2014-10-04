namespace SyslogWeb.Models
{
	public enum SyslogSeverity
	{
		Emergency = 0,
		Alert = 1,
		Critical = 2,
		Error = 3,
		Warning = 4,
		Notice = 5,
		Informational = 6,
		Debug = 7,
		Unknown = -1,
		Crit = -2,
		Err = -3
	}
}