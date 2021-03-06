﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Bootstrap
{
    public class Package
    {
        public Package(string basePath, string name)
        {
            Directory.CreateDirectory(Path.Combine(basePath, name + "-Blue"));
            Directory.CreateDirectory(Path.Combine(basePath, name + "-Green"));
            BluePath = Path.Combine(basePath, name + "-Blue");
            GreenPath = Path.Combine(basePath, name + "-Green");
            PackagePath = Path.Combine(basePath, name + ".zip");
            CachedVersionPath = Path.Combine(basePath, name + ".version.txt");
            ActiveModePath = Path.Combine(basePath, name + ".mode.txt");
            SettingsPath = Path.Combine(basePath, name.ToLowerInvariant() + "-settings.json");
			if (Platform.IsRunningOnMono)
			{
				ExecutableName = name + ".app";
			}
			else
			{
            	ExecutableName = name + ".exe";
			}

            // Try and find the active mode from somewhere...
            if (File.Exists(ActiveModePath))
            {
                try
                {
                    using (var reader = new StreamReader(ActiveModePath))
                    {
                        ActiveMode = reader.ReadToEnd().Trim();
                    }
                }
                catch
                {
                    try
                    {
                        if (File.Exists(Path.Combine(BluePath, ExecutableName)))
                        {
                            ActiveMode = "Blue";
                        }
                        else if (File.Exists(Path.Combine(GreenPath, ExecutableName)))
                        {
                            ActiveMode = "Green";
                        }
                        else
                        {
                            ActiveMode = "Blue";
                        }
                    }
                    catch
                    {
                        ActiveMode = "Blue";
                    }
                }
            }
            else
            {
                try
                {
                    if (File.Exists(Path.Combine(BluePath, ExecutableName)))
                    {
                        ActiveMode = "Blue";
                    }
                    else if (File.Exists(Path.Combine(GreenPath, ExecutableName)))
                    {
                        ActiveMode = "Green";
                    }
                    else
                    {
                        ActiveMode = "Blue";
                    }
                }
                catch
                {
                    ActiveMode = "Blue";
                }
            }

            // Preemptively calculate the current version?
            CalculatePackageVersion();
        }

        public string Version { get; set; }

        public string AvailableVersion { get; private set; }

        public string BluePath { get; private set; }

        public string GreenPath { get; private set; }

        public string ActiveMode { get; set; }

        public string ActiveModePath { get; private set; }

        public string AvailableFile { get; private set; }

        public string PackagePath { get; private set; }

        public string CachedVersionPath { get; private set; }

        public string SettingsPath { get; private set; }

        public Process MonitoredProcess { get; private set; }

        public bool ProcessShouldBeRunning { get; private set; }

        public string ExecutableName { get; private set; }

        public DateTime? LastStartTime { get; private set; }

        public string ActivePath
        {
            get { return ActiveMode == "Blue" ? BluePath : GreenPath; }
        }

        public string InactivePath
        {
            get { return ActiveMode == "Blue" ? GreenPath : BluePath; }
        }

        public void SetAvailableVersions(string availableVersion, string availableFile)
        {
            AvailableVersion = availableVersion;
            AvailableFile = availableFile;
        }

        public void ExtractPackageIfExists()
        {
            if (File.Exists(PackagePath))
            {
                ExtractPackage();
            }
        }

        public async Task UpdateVersion(bool forceUpdate, Action<double> downloadProgress)
        {
            if (!string.IsNullOrWhiteSpace(AvailableVersion))
            {
                if (Version != AvailableVersion || forceUpdate)
                {
                    var package = new WebClient();
                    package.DownloadProgressChanged += (sender, args) =>
                    {
                        downloadProgress(args.BytesReceived/(double) args.TotalBytesToReceive);
                    };
                    await package.DownloadFileTaskAsync(new Uri(AvailableFile), PackagePath);
                    KillUnmonitoredProcesses();
                    CalculatePackageVersion();
                    ExtractPackage();
                    KillProcess();
                    SwapMode();
                    StartProcess();
                }
            }
        }

        private void SwapMode()
        {
            ActiveMode = ActiveMode == "Blue" ? "Green" : "Blue";

            try
            {
                using (var writer = new StreamWriter(ActiveModePath))
                {
                    writer.Write(ActiveMode);
                }
            }
            catch { }
        }

        private void ExtractPackage()
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(PackagePath);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }

                    var entryFileName = zipEntry.Name;

                    var buffer = new byte[4096];
                    var zipStream = zf.GetInputStream(zipEntry);

                    var fullZipToPath = Path.Combine(InactivePath, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            catch (IOException)
            {
                // ignored
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        public bool StartProcess()
        {
            ProcessShouldBeRunning = true;

            var startInfo = new ProcessStartInfo();

            var clientPath = Path.Combine(ActivePath, ExecutableName);
            if (Version != null && File.Exists(clientPath))
            {
                if (Platform.IsRunningOnMono)
                {
                    startInfo.FileName = "/usr/bin/open";
                    startInfo.Arguments = "-a \"" + clientPath + "\"";
                }
                else
                {
                    startInfo.FileName = clientPath;
                }
                startInfo.WorkingDirectory = ActivePath;
            }
            else
            {
                return false;
            }

            startInfo.UseShellExecute = false;

            if (startInfo.FileName == "")
            {
                return false;
            }

            try
            {
                KillUnmonitoredProcesses();
            }
            catch
            {
                // Ignore
            }

            if (LastStartTime != null && LastStartTime.Value > DateTime.UtcNow.AddMinutes(-1))
            {
                // Never restart it again if we tried to start it in the last 60 seconds.
                return false;
            }

            LastStartTime = DateTime.UtcNow;

            MonitoredProcess = new Process();
            MonitoredProcess.StartInfo = startInfo;
            MonitoredProcess.EnableRaisingEvents = true;
            MonitoredProcess.Start();

            return true;
        }

        public void KillProcess()
        {
            ProcessShouldBeRunning = false;

            if (MonitoredProcess == null)
            {
                return;
            }

            if (!MonitoredProcess.HasExited)
            {
                MonitoredProcess.Kill();
                Thread.Sleep(1000);
            }
        }

        public void KillUnmonitoredProcesses()
        {
			if (Platform.IsRunningOnMono)
			{
				return;
			}

            var thisId = Process.GetCurrentProcess().Id;
            foreach (var process in Process.GetProcesses())
            {
                if (process.Id == thisId)
                    continue;
                if (MonitoredProcess != null && !MonitoredProcess.HasExited && process.Id == MonitoredProcess.Id)
                {
                    continue;
                }
                var isMatchingProcess = false;
                try
                {
                    isMatchingProcess = process.MainModule.FileName.EndsWith(ExecutableName,
                        StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    // ignored
                }
                try
                {
                    if (Path.GetFileName(process.MainModule.FileName).Contains("mono"))
                    {
                        // Yay. Messy.
                        if (Platform.IsRunningOnMono && Platform.GetPlatform().Platform != PlatformID.Win32NT)
                        {
                            var ps = new ProcessStartInfo("/bin/ps", $"-p {process.Id} -o command");
                            ps.RedirectStandardOutput=true;
                            ps.UseShellExecute=false;
                            var p = Process.Start(ps);
                            var reader = p.StandardOutput;
                            p.WaitForExit();
                            var output = reader.ReadToEnd();
                            output= output.Trim().Split('\n').Last();
                            while (!File.Exists(output))
                                output = output.Substring(output.IndexOf(' ')).Trim();
                            // Now we've got that out of the way...
							isMatchingProcess = output.EndsWith(ExecutableName.Replace(".exe", ".app"), StringComparison.InvariantCultureIgnoreCase);
                        }
                        else
                        {
                            // Let's just ignore whoever decided this was a good idea. [Probably Katelyn]
                        }
                    }
                }
                catch (Exception c) { }
                if (isMatchingProcess)
                {
                    process.Kill();
                }
            }
        }

        public void CalculatePackageVersion()
        {
            if (!File.Exists(PackagePath))
            {
                Version = null;
                return;
            }

            var success = false;
            Exception cachedException = null;
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    var md5 = MD5.Create();
                    var contentBytes = File.ReadAllBytes(PackagePath);
                    md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

                    Version = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
                    try
                    {
                        using (var writer = new StreamWriter(CachedVersionPath))
                        {
                            writer.Write(Version);
                        }
                    }
                    catch
                    {
                    }
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    cachedException = ex;
                    Thread.Sleep(1000);
                }
            }

            if (!success)
            {
                Version = null;

                if (File.Exists(CachedVersionPath))
                {
                    try
                    {
                        using (var reader = new StreamReader(CachedVersionPath))
                        {
                            Version = reader.ReadToEnd();
                        }
                    }
                    catch
                    {
                    }
                }

                if (Version != null)
                {
                    throw new Exception("Unable to calculation version for " + PackagePath, cachedException);
                }
            }
        }

        public bool RestartMainProcessIfOutOfDate()
        {
            // This is used for the self-update of the bootstrap.  If our available
            // version is set and does not match.
            if (!string.IsNullOrWhiteSpace(AvailableVersion) &&
                AvailableVersion != Version)
            {
                // We have written out into the active version, but we
                // won't be currently running from there.
                var startInfo = new ProcessStartInfo();

                var clientPath = Path.Combine(ActivePath, ExecutableName);
                if (Version != null && File.Exists(clientPath))
                {
                    startInfo.FileName = clientPath;
                    // Pass the current process ID to the new bootstrap so that it will
                    // terminate this one.
                    startInfo.Arguments = Process.GetCurrentProcess().Id.ToString();
                    startInfo.WorkingDirectory = ActivePath;
                }
                else
                {
                    return false;
                }

                startInfo.UseShellExecute = false;

                if (startInfo.FileName == "")
                {
                    return false;
                }

                var process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                return true;
            }

            return false;
        }
    }
}
