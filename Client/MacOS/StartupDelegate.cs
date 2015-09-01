using System;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Client
{
	public partial class StartupDelegate : NSApplicationDelegate
	{
		Manager currentServiceObject;

		public StartupDelegate () { }

		public override void FinishedLaunching (NSObject notification)
		{
			currentServiceObject = new Manager();
			currentServiceObject.Sender = this;		
			currentServiceObject.Run ();
		}
	}
}

