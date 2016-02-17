// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace MonkeyBeacon
{
	[Register ("BeaconViewController")]
	partial class BeaconViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel proximityLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel proximityValueLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (proximityLabel != null) {
				proximityLabel.Dispose ();
				proximityLabel = null;
			}
			if (proximityValueLabel != null) {
				proximityValueLabel.Dispose ();
				proximityValueLabel = null;
			}
		}
	}
}
