using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using CoreLocation;
using Foundation;
using Estimote;
using UIKit;

namespace MonkeyBeacon
{
	partial class BeaconViewController : UIViewController
	{
		BeaconManager beaconManager;
		UtilityManager utilityManager;

		CLLocationManager locationManager;
		CLBeaconRegion region;

		MonkeyService monkeyService;
		ILocalHueClient client;
		string ipAddress;

		private string _pathToDatabase;

		public string IpAddress {
			get {
				return this.ipAddress;
			}
			set {
				ipAddress = value;
			}
		}

		public BeaconViewController (IntPtr handle) : base (handle)
		{
			CurrentPlatform.Init ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//set up sqlite db
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			_pathToDatabase = Path.Combine (documents, "db_sqlite-net.db");

			TryToConnectToBridge ();

			utilityManager = new UtilityManager ();
			beaconManager = new BeaconManager ();

			locationManager = new CLLocationManager ();
			locationManager.RequestWhenInUseAuthorization ();

			beaconManager.AuthorizationStatusChanged += (sender, e) => {
				beaconManager.StartMonitoringForRegion (region);
			};

			beaconManager.ExitedRegion += (sender, e) => {
				proximityValueLabel.Text = "You have EXITED the PooBerry region";
			};

			locationManager.DidDetermineState += (object sender, CLRegionStateDeterminedEventArgs e) => {
				switch (e.State) {
				case CLRegionState.Inside:
					OnAdd ();
					Console.WriteLine ("region state inside");
					break;
				case CLRegionState.Outside:
					Console.WriteLine ("region state outside");
					StartMonitoringBeacons ();
					break;
				case CLRegionState.Unknown:
				default:
					Console.WriteLine ("region state unknown");
					StartMonitoringBeacons ();
					break;
				}
			};

			locationManager.RegionEntered += (object sender, CLRegionEventArgs e) => {
				Console.WriteLine ("beacon region entered");
			};

			#region Hue UI
			connectBridge.TouchUpInside += ConnectBridgeClicked;
			onButton.TouchUpInside += OnButton_TouchUpInside;
			offButton.TouchUpInside += OffButton_TouchUpInside;
			#endregion Hue UI
		}

		async void TryToConnectToBridge ()
		{
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

		#region HueControls
		void OffButton_TouchUpInside (object sender, EventArgs e)
		{
			var command = new LightCommand ();
			command.On = false;
			client.SendCommandAsync (command);
		}

		async void OnButton_TouchUpInside (object sender, EventArgs e)
		{
			if (client != null) {
				var command = new LightCommand ();
				command.On = true;
				await client.SendCommandAsync (command);
			} else {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("Connect to bridge first");
			}
		}

		async void ConnectBridgeClicked (object sender, EventArgs e)
		{
			IBridgeLocator locator = new HttpBridgeLocator ();
			IEnumerable<string> bridgeIPs = await locator.LocateBridgesAsync (TimeSpan.FromSeconds (5));

			IpAddress = bridgeIPs.FirstOrDefault ();

			client = new LocalHueClient (IpAddress);

			var appKey = new HueAppKey{ AppId = await client.RegisterAsync ("pooberry", "iphone") };
			var bridgeIp = new HueBridge { HueBridgeIpAddress = IpAddress };

			var conn = new SQLite.SQLiteConnection (_pathToDatabase);
			//set up bridge table
			conn.CreateTable<HueBridge> ();
			conn.DeleteAll<HueBridge> ();

			//set up app key table
			conn.CreateTable<HueAppKey> ();
			conn.DeleteAll<HueAppKey> ();

			//insert app key and bridge ip into database
			var db = new SQLite.SQLiteConnection (_pathToDatabase);
			db.Insert (bridgeIp);
			db.Insert (appKey);

			var alert = new UIAlertView ("Success!", "Feel free to play with your lights!", null, "OK");		
			alert.Show ();	
		}

		#endregion HueControls

		private void StartMonitoringBeacons ()
		{
			region = new CLBeaconRegion (AppDelegate.BeaconUUID, "BeaconSample");//ushort.Parse ("26547"), ushort.Parse ("56644"),
			region.NotifyOnEntry = true;
			region.NotifyOnExit = true;

			var status = BeaconManager.AuthorizationStatus;
			if (status == CLAuthorizationStatus.NotDetermined) {
				if (!UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
					beaconManager.StartMonitoringForRegion (region);
				} else {
					/*
		             * For more details about the new Location Services authorization model refer to:
		             * https://community.estimote.com/hc/en-us/articles/203393036-Estimote-SDK-and-iOS-8-Location-Services
		             */
					beaconManager.RequestAlwaysAuthorization ();
				}
			} else if (status == CLAuthorizationStatus.Authorized) {
				beaconManager.StartMonitoringForRegion (region);
			} else if (status == CLAuthorizationStatus.Denied) {
				new UIAlertView ("Location Access Denied", "You have denied access to location services. Change this in app settings.", null, "OK").Show ();
			} else if (status == CLAuthorizationStatus.Restricted) {
				new UIAlertView ("Location Not Available", "You have no access to location services.", null, "OK").Show ();
			}
		}

		public override void ViewDidAppear (bool animated)
		{
			StartMonitoringBeacons ();
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			utilityManager.StopEstimoteBeaconDiscovery ();
		}

		async void OnAdd ()
		{
			proximityValueLabel.Text = "You have ENTERED the PooBerry region";

			try {
				monkeyService = new MonkeyService ();
				await monkeyService.InsertTodoItemAsync (new MonkeyItem{ Text = "ENTERED Monkey Region!" });
			} catch (UriFormatException) {
				var alert = new UIAlertView ("Error", "Please make sure you update the applicationURL and applicationKey to match the mobile service you have created.", null, "OK");
				alert.Show ();
				return;		        
			}
		}

		async partial void ColorButton_TouchUpInside (UIButton sender)
		{
			if (client != null) {
				var command = new LightCommand ();
				command.TurnOn ().SetColor ("8e44ad");
				await client.SendCommandAsync (command);
			} else {
				var alert = new UIAlertView ("Hangon!", "First, press the button on the Hue bridge. Then tap 'Connect' in the app.", null, "OK");		
				alert.Show ();		
				Console.WriteLine ("Connect to bridge first");
			}
		}
	}
}