
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Client
{
	public partial class WhoAreYouController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public WhoAreYouController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WhoAreYouController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public WhoAreYouController () : base ("WhoAreYou")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		public Manager AnswerTarget;

		//strongly typed window accessor
		public new WhoAreYou Window {
			get {
				return (WhoAreYou)base.Window;
			}
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			this.Window.Delegate = new MyWindowDelegate ();
		}

		private class MyWindowDelegate : NSWindowDelegate
		{
			public override void WillClose (NSNotification notification)
			{
				NSApplication.SharedApplication.StopModal ();
			}
		}

		partial void goButtonClicked(NSObject sender)
		{
			AnswerTarget.ProvidedName = nameField.StringValue;

			Window.Close();
		}
	}
}

