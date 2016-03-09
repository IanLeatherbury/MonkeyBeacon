using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using Foundation;
using UIKit;
using CoreLocation;
using Q42.HueApi.Interfaces;
using Q42.HueApi;

namespace MonkeyBeacon
{
	partial class RangingVC : UIViewController
	{
		CLBeaconRegion region;
		CLProximity previousProximity;
		CLLocationManager locationManager;
		string message;

		ILocalHueClient client;

		private string _pathToDatabase;

		public RangingVC (IntPtr handle) : base (handle)
		{

			//set up sqlite db
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			_pathToDatabase = Path.Combine (documents, "db_sqlite-net.db");

			region = new CLBeaconRegion (AppDelegate.BeaconUUID, "BeaconSample");//ushort.Parse ("26547"), ushort.Parse ("56644"),
			region.NotifyOnEntry = true;
			region.NotifyOnExit = true;

			locationManager = new CLLocationManager ();
			locationManager.RequestWhenInUseAuthorization ();

			locationManager.DidRangeBeacons += (object sender, CLRegionBeaconsRangedEventArgs e) => {
				if (e.Beacons.Length > 0) {

					CLBeacon beacon = e.Beacons [0];

					switch (beacon.Proximity) {
					case CLProximity.Immediate:
						message = "Immediate";
						break;
					case CLProximity.Near:
						message = "Near";
						break;
					case CLProximity.Far:
						message = "Far";
						break;
					case CLProximity.Unknown:
						message = "Unknown";
						break;
					}

					if (previousProximity != beacon.Proximity) {
						Console.WriteLine (message);
					}
					previousProximity = beacon.Proximity;
				}

			};

			locationManager.StartRangingBeacons (region);

			var db = new SQLite.SQLiteConnection (_pathToDatabase);
			var bridgeIp = db.Table<HueBridge> ().ToArray ();
			client = new LocalHueClient (bridgeIp [0].HueBridgeIpAddress);
			client.Initialize ("pooberry");
		}

		partial void UIButton884_TouchUpInside (UIButton sender)
		{
			Console.WriteLine ("Pushed!");

			var command = new LightCommand ();
			command.On = false;
			client.SendCommandAsync (command);
		}
	}
}
