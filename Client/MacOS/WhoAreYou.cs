
using System;
using System.Collections.Generic;
using System.Linq;
#if PLATFORM_MACOS_LEGACY
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace Client
{
	public partial class WhoAreYou : NSWindow
	{
		#region Constructors

		// Called when created from unmanaged code
		public WhoAreYou (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WhoAreYou (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion
	}
}

