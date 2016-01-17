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
			if (File.Exists("user.txt"))
			{
				using (var reader = new StreamReader("user.txt"))
				{
					this.m_Name = reader.ReadToEnd().Trim();
				}
			}
			else
			{
				// Ask the person who they are ;)
				WhoAreYou way = new WhoAreYou();
				if (way.ShowDialog() != DialogResult.OK)
				{
					Environment.Exit(1);
					return;
				}
				this.m_Name = way.Name;
				way.Dispose();
				GC.Collect();

				using (var writer = new StreamWriter("user.txt"))
				{
					writer.Write(this.m_Name);
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
			this._trayIcon = new TrayIcon(this);
		}

		private void SetTrayIconToCountdown()
		{
			this._trayIcon.Icon = Resources.TrayCountdown;
		}

		private void SetTrayIconToOff()
		{
			this._trayIcon.Icon = Resources.TrayOff;
		}

		private void SetTrayIconToOn()
		{
			this._trayIcon.Icon = Resources.TrayOn;
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