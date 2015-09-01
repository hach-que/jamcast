// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Client
{
	[Register ("WhoAreYouController")]
	partial class WhoAreYouController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField nameField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField promptLabel { get; set; }

		[Outlet]
		Client.WhoAreYou whoAreYouWindow { get; set; }

		[Action ("goButtonClicked:")]
		partial void goButtonClicked (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (nameField != null) {
				nameField.Dispose ();
				nameField = null;
			}

			if (promptLabel != null) {
				promptLabel.Dispose ();
				promptLabel = null;
			}

			if (whoAreYouWindow != null) {
				whoAreYouWindow.Dispose ();
				whoAreYouWindow = null;
			}
		}
	}

	[Register ("WhoAreYou")]
	partial class WhoAreYou
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
