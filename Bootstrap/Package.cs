﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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
            SettingsPath = Path.Combine(basePath, name.ToLowerInvariant() + "-settings.json");
            ExecutableName = name + ".exe";
            ActiveMode = "Blue";
        }

        public string Version { get; private set; }

        public string AvailableVersion { get; private set; }

        public string BluePath { get; private set; }

        public string GreenPath { get; private set; }

        public string ActiveMode { get; set; }

        public string AvailableFile { get; private set; }

        public string PackagePath { get; private set; }

        public string SettingsPath { get; private set; }

        public Process MonitoredProcess { get; private set; }

        public bool ProcessShouldBeRunning { get; private set; }

        public string ExecutableName { get; private set; }

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
                    ExtractPackage();
                    Version = CalculatePackageVersion();
                    KillProcess();
                    SwapMode();
                    StartProcess();
                }
            }
        }

        private void SwapMode()
        {
            ActiveMode = ActiveMode == "Blue" ? "Green" : "Blue";
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
                startInfo.FileName = clientPath;
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

            MonitoredProcess = new Process();
            MonitoredProcess.StartInfo = startInfo;
            MonitoredProcess.EnableRaisingEvents = true;
            MonitoredProcess.Exited += (sender, args) =>
            {
                if (ProcessShouldBeRunning)
                {
                    StartProcess();
                }
            };
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
            foreach (var process in Process.GetProcesses())
            {
                var isMatchingProcess = false;
                try
                {
                    isMatchingProcess = process.MainModule.FileName.StartsWith(InactivePath,
                        StringComparison.InvariantCultureIgnoreCase);
                }
                catch
                {
                    // ignored
                }
                if (isMatchingProcess)
                {
                    process.Kill();
                }
            }
        }

        public string CalculatePackageVersion()
        {
            if (!File.Exists(PackagePath))
            {
                return null;
            }

            var md5 = MD5.Create();
            var contentBytes = File.ReadAllBytes(PackagePath);
            md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
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
            }

            return false;
        }
    }
}
