#if PLATFORM_MACOS

using System;
using System.Drawing;
using MonoMac.AppKit;
using System.Diagnostics;
using MonoMac.Foundation;
using System.IO;
using MonoMac.CoreGraphics;
using System.Runtime.InteropServices;

namespace Client
{
	public partial class Manager
	{
		private NSStatusItem statusBarIcon;

		public NSObject Sender;

		public string ProvidedName = null;

		private static Action onExit;

		public static void PerformExit()
		{
			if (onExit != null) {
				onExit ();
			}

			Environment.Exit (0);
		}

		private void LoadUsername()
		{
			var dialog = new WhoAreYouController();
			dialog.AnswerTarget = this;
			NSApplication.SharedApplication.RunModalForWindow (dialog.Window);

			if (ProvidedName == null)
			{
				PerformExit();
			}

			this.m_Name = ProvidedName;
		}

		private void ListenForApplicationExit(Action onExitMethod)
		{
			onExit = onExitMethod;
		}

		private void ConfigureSystemTrayIcon()
		{
			// Construct menu that will be displayed when tray icon is clicked
			var notifyMenu = new NSMenu();
			var nameItem = new NSMenuItem ("Name: " + m_Name);
			nameItem.Enabled = false;
			var exitMenuItem = new NSMenuItem("Exit JamCast", 
				(a,b) => PerformExit ()); // Just add 'Quit' command

			notifyMenu.AddItem (nameItem);
			notifyMenu.AddItem (NSMenuItem.SeparatorItem);
			notifyMenu.AddItem(exitMenuItem);

			statusBarIcon = NSStatusBar.SystemStatusBar.CreateStatusItem(30);
			statusBarIcon.Menu = notifyMenu;
			statusBarIcon.HighlightMode = true;
			SetTrayIconToOff ();
		}

		private void SetTrayIconToCountdown()
		{
			statusBarIcon.Image = NSImage.FromStream(System.IO.File.OpenRead(
				NSBundle.MainBundle.ResourcePath + @"/MacOS/TrayCountdown.icns"));
		}

		private void SetTrayIconToOff()
		{
			statusBarIcon.Image = NSImage.FromStream(System.IO.File.OpenRead(
				NSBundle.MainBundle.ResourcePath + @"/MacOS/TrayOff.icns"));
		}

		private void SetTrayIconToOn()
		{
			statusBarIcon.Image = NSImage.FromStream(System.IO.File.OpenRead(
				NSBundle.MainBundle.ResourcePath + @"/MacOS/TrayOn.icns"));
		}

		private void ScheduleTrayIconToOff()
		{
		}

		[DllImport("/System/Library/Frameworks/ApplicationServices.framework/Versions/A/Frameworks/CoreGraphics.framework/CoreGraphics")]
		private static extern IntPtr CGWindowListCreateImage(RectangleF screenBounds, CGWindowListOption windowOption, uint windowID, CGWindowImageOption imageOption);

		public Bitmap GetScreen()
		{
			var ptr = CGWindowListCreateImage(new RectangleF(0, 0, 1920, 1080), CGWindowListOption.All, 0, CGWindowImageOption.Default);
			var screenImage = new CGImage(ptr);

			using (var imageRep = new NSBitmapImageRep (screenImage)) {
				var props = NSDictionary.FromObjectAndKey (new NSNumber (1.0), new NSString ("NSImageCompressionFactory"));
				using (var bmpData = imageRep.RepresentationUsingTypeProperties (NSBitmapImageFileType.Png, props)) {
					return (Bitmap)Image.FromStream (bmpData.AsStream());
				}
			}
		}
	}
}

#endif