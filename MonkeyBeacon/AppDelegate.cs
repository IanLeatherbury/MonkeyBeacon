using Foundation;
using UIKit;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;

namespace MonkeyBeacon
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		const string PROXIMITY_UUID = "B9407F30-F5F8-466E-AFF9-25556B57FE6D";

		UIWindow window;
		BeaconViewController viewController;

		public static NSUuid BeaconUUID
		{
			//Virtual
			get { return new NSUuid (PROXIMITY_UUID); }
		}
		public override UIWindow Window {
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			// registers for push for iOS8
			var settings = UIUserNotificationSettings.GetSettingsForTypes(
				UIUserNotificationType.Alert
				| UIUserNotificationType.Badge
				| UIUserNotificationType.Sound,
				new NSSet());

			UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			UIApplication.SharedApplication.RegisterForRemoteNotifications();

			return true;
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			MobileServiceClient client = MonkeyService.DefaultService.GetClient;

			const string templateBodyAPNS = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";

			JObject templates = new JObject();
			templates["genericMessage"] = new JObject
			{
				{"body", templateBodyAPNS}
			};

			// Register for push with your mobile app
			var push = client.GetPush();
			push.RegisterAsync(deviceToken, templates);
		}

		public override void DidReceiveRemoteNotification (UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			NSDictionary aps = userInfo.ObjectForKey(new NSString("aps")) as NSDictionary;

			string alert = string.Empty;
			if (aps.ContainsKey(new NSString("alert")))
				alert = (aps [new NSString("alert")] as NSString).ToString();

			//show alert
			if (!string.IsNullOrEmpty(alert))
			{
				UIAlertView avAlert = new UIAlertView("Notification", alert, null, "OK", null);
				avAlert.Show();
			}
		}
	}
}


