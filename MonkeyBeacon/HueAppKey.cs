using System;
using SQLite;

namespace MonkeyBeacon
{
	public class HueAppKey
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string AppId { get; set; }
	}
}

