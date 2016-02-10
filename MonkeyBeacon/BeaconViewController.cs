using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Estimote;
using CoreLocation;

namespace MonkeyBeacon
{
	partial class BeaconViewController : UIViewController
	{
		BeaconManager beaconManager;
		UtilityManager utilityManager;
		CLBeaconRegion region;
		CLBeacon[] beacons;

		public BeaconViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Title = "Select Beacon";

			utilityManager = new UtilityManager ();
			beaconManager = new BeaconManager ();
			beaconManager.ReturnAllRangedBeaconsAtOnce = true;
			region = new CLBeaconRegion (AppDelegate.BeaconUUID, "BeaconSample");

			beaconManager.AuthorizationStatusChanged += (sender, e) => 
						StartRangingBeacons ();
			beaconManager.RangedBeacons += (sender, e) => {
				beacons = e.Beacons;

				if (e.Beacons.Length == 0)
					return;

				beaconLabel.Text = TextForProximity(e.Beacons[0].Proximity);
			};
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidLoad ();

			StartRangingBeacons ();
		}

		private void StartRangingBeacons ()
		{
			var status = BeaconManager.AuthorizationStatus;
			if (status == CLAuthorizationStatus.NotDetermined) {
				if (!UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
					/*
             * No need to explicitly request permission in iOS < 8, will happen automatically when starting ranging.
             */
					beaconManager.StartRangingBeaconsInRegion (region);

				} else {
					/*
             * Request permission to use Location Services. (new in iOS 8)
             * We ask for "always" authorization so that the Notification Demo can benefit as well.
             * Also requires NSLocationAlwaysUsageDescription in Info.plist file.
             *
             * For more details about the new Location Services authorization model refer to:
             * https://community.estimote.com/hc/en-us/articles/203393036-Estimote-SDK-and-iOS-8-Location-Services
             */
					beaconManager.RequestAlwaysAuthorization ();
				}
			} else if (status == CLAuthorizationStatus.Authorized) {
				beaconManager.StartRangingBeaconsInRegion (region);

			} else if (status == CLAuthorizationStatus.Denied) {
				new UIAlertView ("Location Access Denied", "You have denied access to location services. Change this in app settings.", null, "OK").Show ();
			} else if (status == CLAuthorizationStatus.Restricted) {
				new UIAlertView ("Location Not Available", "You have no access to location services.", null, "OK").Show ();
			}
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			beaconManager.StopRangingBeaconsInRegion (region);
			utilityManager.StopEstimoteBeaconDiscovery ();
		}

		private string TextForProximity(CLProximity proximity)
		{
			switch (proximity) {
			case CLProximity.Far:
				return "Far";
			case CLProximity.Immediate:
				return "Immediate";
			case CLProximity.Near:
				return "Near";
			default:
				return "Unknown";
			}
		}
	}

}