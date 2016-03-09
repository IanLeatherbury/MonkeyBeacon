using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using Foundation;
using UIKit;
using CoreLocation;
using Q42.HueApi.Interfaces;
using Q42.HueApi;
using System.Collections.Generic;

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
			SetUpHue ();



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
						SetImmediateColor();
						message = "Immediate";
						break;
					case CLProximity.Near:
						message = "Near";
						SetNearColor();
						break;
					case CLProximity.Far:
						message = "Far";
						SetFarColor();
						break;
					case CLProximity.Unknown:
						message = "Unknown";
						SetUnknownColor();
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

		async void SetImmediateColor(){
			if (client != null) {
				var command = new LightCommand ();
				command.TurnOn ().SetColor ("c0392b");
				await client.SendCommandAsync (command);
			} else {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("Connect to bridge first");
			}
		}

		async void SetNearColor(){
			if (client != null) {
				var command = new LightCommand ();
				command.TurnOn ().SetColor ("e67e22");
				await client.SendCommandAsync (command);
			} else {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("Connect to bridge first");
			}
		}

		async void SetFarColor(){
			if (client != null) {
				var command = new LightCommand ();
				command.TurnOn ().SetColor ("2980b9");
				await client.SendCommandAsync (command);
			} else {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("Connect to bridge first");
			}
		}

		async void SetUnknownColor()
		{
			if (client != null) {
				//turns lights off
				var command = new LightCommand ();
				command.On = false;
				await client.SendCommandAsync (command);
			} else {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("Connect to bridge first");
			}
		}

		async partial void UIButton884_TouchUpInside (UIButton sender)
		{
			Console.WriteLine ("Pushed!");

			if (client != null) {
				var command = new LightCommand ();
				command.TurnOn ().SetColor ("FF00AA");
				await client.SendCommandAsync (command);
			} else {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("Connect to bridge first");
			}
		}

		async void SetUpHue(){

			IBridgeLocator locator = new HttpBridgeLocator ();
			IEnumerable<string> bridgeIPs = await locator.LocateBridgesAsync (TimeSpan.FromSeconds (5));

			var db = new SQLite.SQLiteConnection (_pathToDatabase);
			try {
				var bridgeIp = db.Table<HueBridge> ().ToArray ();
				var appKey = db.Table<HueAppKey> ().ToArray ();

				client = new LocalHueClient (bridgeIp [0].HueBridgeIpAddress);
				client.Initialize (appKey [0].AppId);
			} catch {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("First, connect to bridge");
			}


		}
	}
}
