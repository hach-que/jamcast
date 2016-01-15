#if PLATFORM_MACOS

using System;
using System.Drawing;
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
#else
using AppKit;
using Foundation;
using CoreGraphics;
#endif
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Client
{
	public partial class Manager
	{
		private NSStatusItem statusBarIcon;

		public NSObject Sender;

		public string ProvidedName = null;

		private static Action onExit;

        private Dictionary<IPAddress, Process> _streamingProcesses = new Dictionary<IPAddress, Process>();

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

	    private void StartStreaming(IPAddress address, out string sdp, Action onProcessExit)
	    {
	        //var controller = new FfmpegStreamController();
	        //var process = controller.StreamToTarget(address.ToString(), out sdp);
	        //process.Exited += (sender, args) =>
	        //{
	        //    onProcessExit();
	        //};
	        //_streamingProcesses[address] = process;
	    }

	    private void StopStreaming(IPAddress address)
	    {
	        //if (_streamingProcesses[address] != null && !_streamingProcesses[address].HasExited)
	        //{
	        //    _streamingProcesses[address].Kill();
	        //}
	    }
	}
}

#endif
