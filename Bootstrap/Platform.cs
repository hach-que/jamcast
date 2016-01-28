using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrap
{
    class Platform
    {
        public  static readonly string AssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly bool IsRunningOnMono = (Type.GetType ("Mono.Runtime") != null);

        public static OperatingSystem GetPlatform()
        {
            switch (Environment.OSVersion.Platform) {

                case PlatformID.Unix:
                    if (Directory.Exists ("/Library"))
                        return new OperatingSystem (PlatformID.MacOSX, Environment.OSVersion.Version);
                    else
                        return Environment.OSVersion;
                    break;

                case PlatformID.MacOSX: // Silverlight or CoreCLR?
                // Mono is never going to get here, because of this code:
                // https://github.com/mono/mono/blob/9e396e4022a4eefbcdeeae1d86c03afbf04043b7/mcs/class/corlib/System/Environment.cs#L239
                case PlatformID.Win32NT:
                default:
                    return Environment.OSVersion;

            }
        }

        private static string _monopath;

        public static string MonoPath
        {
            get
            {
                if (String.IsNullOrEmpty(_monopath))
                {
                    foreach (var path in new string[] 
                    {
                        "/usr/local/bin/mono",
                        "/usr/bin/mono",
                        "/bin/mono",
                    })
                    {
                        if (File.Exists(path))
                        {
                            _monopath = path;
                            break;
                        }
                    }
                }
                return _monopath;
            } 
        }
    }
}
