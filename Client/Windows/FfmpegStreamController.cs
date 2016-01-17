using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Client.Windows
{
    public class FfmpegStreamController
    {
        public Process StreamToTarget(string ip, out string sdp)
        {
            var ffmpeg = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName, "ffmpeg.exe");
            var processStartInfo = new ProcessStartInfo(ffmpeg, "-re -f gdigrab -framerate 30 -i desktop -f rtp rtp://" + ip + ":1234");
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            var process = Process.Start(processStartInfo);
            process.EnableRaisingEvents = true;
            sdp = string.Empty;
            string previousLine = null;
            string line = null;
            while (true)
            {
                previousLine = line;
                line = process.StandardOutput.ReadLine().Trim();
                if (line != string.Empty)
                {
                    sdp += line + "\r\n";
                }
                if (string.IsNullOrWhiteSpace(line) && string.IsNullOrWhiteSpace(previousLine))
                {
                    break;
                }
            }
            return process;
        }
    }
}
