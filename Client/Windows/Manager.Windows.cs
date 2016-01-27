#if PLATFORM_WINDOWS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Client.Properties;
using System.Net;
using Client.Windows;

namespace Client
{
	public partial class Manager
	{
		private TrayIcon _trayIcon = null;

        private Dictionary<IPAddress, Process> _streamingProcesses = new Dictionary<IPAddress, Process>();

		private void LoadUsername()
		{
		    var userPath = Path.Combine(
		        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		        "JamCast",
		        "user.txt");
		    Directory.CreateDirectory(
		        Path.Combine(
		            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		            "JamCast"));

			if (File.Exists(userPath))
			{
				using (var reader = new StreamReader(userPath))
				{
					_name = reader.ReadLine()?.Trim();
				    _email = reader.ReadLine()?.Trim();
				}
			}
			
            if (string.IsNullOrWhiteSpace(_name) || string.IsNullOrWhiteSpace(_email))
			{
				// Ask the person who they are ;)
				AuthForm way = new AuthForm();
				if (way.ShowDialog() != DialogResult.OK)
				{
					Environment.Exit(1);
					return;
				}
				_name = way.AuthResult.FullName;
                _email = way.AuthResult.EmailAddress;
				way.Dispose();
				GC.Collect();

				using (var writer = new StreamWriter(userPath))
				{
					writer.WriteLine(_name);
                    writer.WriteLine(_email);
				}
			}
		}

		private void ListenForApplicationExit(Action onExit)
		{
			// Listen for the application exit event.
			Application.ApplicationExit += (sender, e) => onExit();
		}

		private void ConfigureSystemTrayIcon()
		{
			// Show the system tray icon.
			_trayIcon = new TrayIcon(this);
		}

		private void SetTrayIconToCountdown()
		{
			_trayIcon.Icon = Resources.TrayCountdown;
		}

		private void SetTrayIconToOff()
		{
			_trayIcon.Icon = Resources.TrayOff;
		}

		private void SetTrayIconToOn()
		{
			_trayIcon.Icon = Resources.TrayOn;
		}

	    private void StartStreaming(IPAddress address, out string sdp, Action onProcessExit)
	    {
	        var controller = new FfmpegStreamController();
	        var process = controller.StreamToTarget(address.ToString(), out sdp);
	        process.Exited += (sender, args) =>
	        {
	            onProcessExit();
	        };
	        _streamingProcesses[address] = process;
	    }

	    private void StopStreaming(IPAddress address)
	    {
	        if (_streamingProcesses.ContainsKey(address))
	        {
	            if (_streamingProcesses[address] != null && !_streamingProcesses[address].HasExited)
	            {
	                _streamingProcesses[address].Kill();
	                _streamingProcesses.Remove(address);
	            }
	        }
	    }
	}
}

#endif