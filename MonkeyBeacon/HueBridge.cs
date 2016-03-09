using System;
using SQLite;

namespace MonkeyBeacon
{
	public class HueBridge
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string HueBridgeIpAddress {get; set;}
	}
}

