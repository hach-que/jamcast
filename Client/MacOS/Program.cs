﻿#if PLATFORM_MACOS
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
#else
using AppKit;
#endif

namespace Client
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			Run(args);
		}

		public static void Run(string[] args)
		{
			NSApplication.Init();

			//Manager m = new Manager();
			//m.Run();

			NSApplication.SharedApplication.Delegate = new StartupDelegate();
			NSApplication.SharedApplication.Run();
		}
	}
}
#endif