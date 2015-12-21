using System;
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
using MonoMac.Foundation;
#else
using AppKit;
using Foundation;
#endif

namespace Client
{
	public partial class StartupDelegate : NSApplicationDelegate
	{
		Manager currentServiceObject;

		public StartupDelegate () { }

		#if PLATFORM_MACOS_LEGACY
		public override void FinishedLaunching (NSObject notification)
		#else
		public override void DidFinishLaunching (NSNotification notification)
		#endif
		{
			currentServiceObject = new Manager();
			currentServiceObject.Sender = this;		
			currentServiceObject.Run ();
		}
	}
}

