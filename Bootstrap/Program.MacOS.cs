#if PLATFORM_MACOS
using System;
using System.Threading;
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
using MonoMac.Foundation;
#else
using AppKit;
using Foundation;
#endif

namespace Bootstrap
{
    public static partial class Program
	{
		public static NSMenuItem _lastContactMenuItem;
		public static NSMenuItem _roleMenuItem;
		public static NSMenuItem _statusMenuItem;
		public static NSMenuItem _pingStatusMenuItem;
		public static NSMenuItem _bootstrapVersionMenuItem;
		public static NSMenuItem _clientVersionMenuItem;
		public static NSMenuItem _projectorVersionMenuItem;
		public static NSMenuItem _bootstrapAvailableVersionMenuItem;
		public static NSMenuItem _clientAvailableVersionMenuItem;
		public static NSMenuItem _projectorAvailableVersionMenuItem;
		public static NSMenuItem _cloudOperationsMenuItem;
		public static NSMenuItem _threadWaitForMessagesMenuItem;
		public static NSMenuItem _threadUpdateContextMenuMenuItem;
		public static NSMenuItem _threadApplicationPumpMenuItem;
		public static NSMenuItem _sendTimeMenuItem;
		public static NSMenuItem _lastRecieveTimeMenuItem;

        public static void PlatformTraySetup()
        {
		}

		public static string[] ProgramArgs;

		internal static void Main(string[] args)
		{
			ProgramArgs = args;

			NSApplication.Init();

			ThreadApplication = new Thread(() =>
				{
					Program.InternalMain(Program.ProgramArgs);
				});
			ThreadApplication.IsBackground = true;
			ThreadApplication.Start();

			var application = NSApplication.SharedApplication;
			application.Delegate = new AppDelegate();
			application.Run();
		}

		public static NSMenu SetupMenu()
		{
			var menu = new NSMenu();

			menu.AddItem(new NSMenuItem("JamCast Bootstrap") { Enabled = false });
			menu.AddItem(NSMenuItem.SeparatorItem);
			menu.AddItem(Program._lastContactMenuItem = new NSMenuItem("Last Contact: " + (LastContact == null ? "Never" : LastContact.ToString())) { Enabled = false });
			menu.AddItem(Program._roleMenuItem = new NSMenuItem("Role: " + Role) { Enabled = false });
			menu.AddItem(Program._statusMenuItem = new NSMenuItem("Status: " + Status) { Enabled = false });
			menu.AddItem(Program._pingStatusMenuItem = new NSMenuItem("Ping Status: " + Status) { Enabled = false });
			menu.AddItem(Program._cloudOperationsMenuItem = new NSMenuItem("Cloud Operations: " + (PubSub == null ? 0 : PubSub.OperationsRequested)) { Enabled = false });
			menu.AddItem(NSMenuItem.SeparatorItem);
			menu.AddItem(Program._threadWaitForMessagesMenuItem = new NSMenuItem("Thread - Wait for Messages: " + GetStatusForThread(ThreadWaitForMessages)) { Enabled = false });
			menu.AddItem(Program._threadUpdateContextMenuMenuItem = new NSMenuItem("Thread - Update Context Menu: " + GetStatusForThread(ThreadUpdateContextMenu)) { Enabled = false });
			menu.AddItem(Program._threadApplicationPumpMenuItem = new NSMenuItem("Thread - Application Pump: " + GetStatusForThread(ThreadApplication)) { Enabled = false });
			menu.AddItem(NSMenuItem.SeparatorItem);
			menu.AddItem(Program._sendTimeMenuItem = new NSMenuItem("Last Message Sent: " + _sendTime) { Enabled = false });
			menu.AddItem(Program._lastRecieveTimeMenuItem = new NSMenuItem("Last Message Acked: " + (_lastRecieveTime == null ? "(no message recieved with timestamp yet)" : _lastRecieveTime.ToString())) { Enabled = false });
			menu.AddItem(NSMenuItem.SeparatorItem);
			menu.AddItem(Program._bootstrapVersionMenuItem = new NSMenuItem("Bootstrap Version: " + (Bootstrap == null ? "..." : Bootstrap.Version)) { Enabled = false });
			menu.AddItem(Program._clientVersionMenuItem = new NSMenuItem("Client Version: " + (Client == null ? "..." : Client.Version)) { Enabled = false });
			menu.AddItem(Program._projectorVersionMenuItem = new NSMenuItem("Projector Version: " + (Projector == null ? "..." : Projector.Version)) { Enabled = false });
			menu.AddItem(NSMenuItem.SeparatorItem);
			menu.AddItem(Program._bootstrapAvailableVersionMenuItem = new NSMenuItem("Bootstrap Available Version: " + (Bootstrap == null ? "..." : Bootstrap.AvailableVersion)) { Enabled = false });
			menu.AddItem(Program._clientAvailableVersionMenuItem = new NSMenuItem("Client Available Version: " + (Client == null ? "..." : Client.AvailableVersion)) { Enabled = false });
			menu.AddItem(Program._projectorAvailableVersionMenuItem = new NSMenuItem("Projector Available Version: " + (Projector == null ? "..." : Projector.AvailableVersion)) { Enabled = false });
			menu.AddItem(NSMenuItem.SeparatorItem);
			menu.AddItem(new NSMenuItem("Quit", (a, b) => { NSApplication.SharedApplication.Terminate(menu); }));

			ThreadUpdateContextMenu = new Thread(() =>
				{
					while (Thread.CurrentThread.IsAlive)
					{
						menu.InvokeOnMainThread(() =>
							{
						_lastContactMenuItem.Title = "Last Contact: " +
							(LastContact == null ? "Never" : LastContact.ToString());
						_roleMenuItem.Title = "Role: " + Role;
						_statusMenuItem.Title = "Status: " + Status;
						_pingStatusMenuItem.Title = "Ping Status: " + PingStatus;
						_cloudOperationsMenuItem.Title = "Cloud Operations: " + (PubSub == null ? 0 : PubSub.OperationsRequested);
						_statusMenuItem.Title = "Status: " + Status;

						_threadWaitForMessagesMenuItem.Title = "Thread - Wait for Messages: " + GetStatusForThread(ThreadWaitForMessages);
						_threadUpdateContextMenuMenuItem.Title = "Thread - Update Context Menu: " + GetStatusForThread(ThreadUpdateContextMenu);
						_threadApplicationPumpMenuItem.Title = "Thread - Application Pump: " + GetStatusForThread(ThreadApplication);

						_bootstrapVersionMenuItem.Title = "Bootstrap Version: " +
							(Bootstrap == null ? "..." : Bootstrap.Version);
						_clientVersionMenuItem.Title = "Client Version: " +
							(Client == null ? "..." : Client.Version);
						_projectorVersionMenuItem.Title = "Projector Version: " +
							(Projector == null ? "..." : Projector.Version);

						_bootstrapAvailableVersionMenuItem.Title = "Bootstrap Available Version: " +
							(Bootstrap == null ? "..." : Bootstrap.AvailableVersion);
						_clientAvailableVersionMenuItem.Title = "Client Available Version: " + 
							(Client == null ? "..." : Client.AvailableVersion);
						_projectorAvailableVersionMenuItem.Title = "Projector Available Version: " +
							(Projector == null ? "..." : Projector.AvailableVersion);

						_sendTimeMenuItem.Title = "Last Message Sent: " + _sendTime;
						_lastRecieveTimeMenuItem.Title = "Last Message Acked: " + (_lastRecieveTime == null ? "(no message recieved with timestamp yet)" : _lastRecieveTime.ToString());

						/*
						if (LastContact == null || LastContact < DateTime.Now.AddSeconds(-60))
						{
							_trayIcon.Icon = _trayIconNoContact;
						}
						else
						{
							if (Status.StartsWith("Updating"))
							{
								if (_trayIcon.Icon == _trayIconNormal)
								{
									_trayIcon.Icon = _trayIconDownloading;
								}
								else
								{
									_trayIcon.Icon = _trayIconNormal;
								}
							}
							else
							{
								_trayIcon.Icon = _trayIconNormal;
							}
						}*/
							});

						Thread.Sleep(500);
					}
				})
			{ IsBackground = true };
			ThreadUpdateContextMenu.Start();

			return menu;
		}

		private static string GetStatusForThread(Thread thread)
		{
			if (thread == null)
			{
				return "Not Created";
			}

			try
			{
				return thread.ThreadState.ToString();
			}
			catch (Exception)
			{
				return "(Exception while getting status)";
			}
		}
    }

	[Register("AppDelegate")]
	public partial class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate () { }

		private NSStatusItem _item;

#if PLATFORM_MACOS_LEGACY
		public override void FinishedLaunching (NSObject notification)
#else
		public override void DidFinishLaunching (NSNotification notification)
#endif
		{
			var notifyMenu = Program.SetupMenu();

			// Construct menu that will be displayed when tray icon is clicked
			/*var notifyMenu = new NSMenu();
			var exitMenuItem = new NSMenuItem("Quit My Application", 
				(a,b) => { System.Environment.Exit(0); }); // Just add 'Quit' command
			notifyMenu.AddItem(exitMenuItem);
*/
			// Display tray icon in upper-right-hand corner of the screen
			_item = NSStatusBar.SystemStatusBar.CreateStatusItem(30);
			_item.Menu = notifyMenu;
			_item.Image = NSImage.FromStream(System.IO.File.OpenRead(
				NSBundle.MainBundle.ResourcePath + @"/satellite-16.icns"));
			_item.HighlightMode = true;

			// Remove the system tray icon from upper-right hand corner of the screen
			// (works without adjusting the LSUIElement setting in Info.plist)
			//NSApplication.SharedApplication.ActivationPolicy = 
	//			NSApplicationActivationPolicy.Accessory;
			
		}
	}
}

#endif