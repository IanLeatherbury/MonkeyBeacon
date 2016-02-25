using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Estimote;
using CoreLocation;
using Microsoft.WindowsAzure.MobileServices;

namespace MonkeyBeacon
{
	partial class BeaconViewController : UIViewController
	{
		BeaconManager beaconManager;
		UtilityManager utilityManager;
		CLBeaconRegion region;
		MonkeyService monkeyService;

		public BeaconViewController (IntPtr handle) : base (handle)
		{
			CurrentPlatform.Init ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			try {
				monkeyService = new MonkeyService ();
				monkeyService.InsertTodoItemAsync (new MonkeyItem{ Text = "New Monkey Item!" });
			} catch (UriFormatException) {
				var alert = new UIAlertView ("Error", "Please make sure you update the applicationURL and applicationKey to match the mobile service you have created.", null, "OK");
				alert.Show ();
				return;		        
			}

			utilityManager = new UtilityManager ();

			beaconManager = new BeaconManager ();

			region = new CLBeaconRegion (AppDelegate.BeaconUUID, ushort.Parse ("26547"), ushort.Parse ("56644"), "BeaconSample");
			region.NotifyOnEntry = true;
			region.NotifyOnExit = true;

			beaconManager.AuthorizationStatusChanged += (sender, e) => {
				beaconManager.StartMonitoringForRegion (region);
			};

			beaconManager.ExitedRegion += (sender, e) => {
				var notification = new UILocalNotification ();
				notification.AlertBody = "Exit region notification";
				UIApplication.SharedApplication.PresentLocalNotificationNow (notification);
				proximityValueLabel.Text = "You have EXITED the PooBerry region";
			};

			beaconManager.EnteredRegion += (sender, e) => {
				var notification = new UILocalNotification ();
				notification.AlertBody = "Enter region notification";
				UIApplication.SharedApplication.PresentLocalNotificationNow (notification);
				proximityValueLabel.Text = "You have ENTERED the PooBerry region";
				OnAdd ();
			};
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidLoad ();
			StartMonitoringBeacons ();
		}

		private void StartMonitoringBeacons ()
		{
			var status = BeaconManager.AuthorizationStatus;
			if (status == CLAuthorizationStatus.NotDetermined) {
				if (!UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
					/*
             * No need to explicitly request permission in iOS < 8, will happen automatically when starting ranging.
             */
					beaconManager.StartMonitoringForRegion (region);

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
				beaconManager.StartMonitoringForRegion (region);
			} else if (status == CLAuthorizationStatus.Denied) {
				new UIAlertView ("Location Access Denied", "You have denied access to location services. Change this in app settings.", null, "OK").Show ();
			} else if (status == CLAuthorizationStatus.Restricted) {
				new UIAlertView ("Location Not Available", "You have no access to location services.", null, "OK").Show ();
			}
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			utilityManager.StopEstimoteBeaconDiscovery ();
		}

		#region UI Actions

		async void OnAdd ()
		{
			var newItem = new MonkeyItem {
				Text = "First commit!"
			};

			await monkeyService.InsertTodoItemAsync (newItem);
		}

		#endregion
	}
}