using System;
using Newtonsoft.Json;

namespace MonkeyBeacon
{
	public class MonkeyItem
	{
		public string Id { get; set; }

		[JsonProperty(PropertyName = "text")]
		public string Text { get; set; }
	}
}

