using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Bootstrap
{
    class Prerun
    {
        public static readonly bool IsRunningOnMono = (Type.GetType ("Mono.Runtime") != null);

        internal static void Main(string[] args)
        {
            if (IsRunningOnMono) // It's everyone's favorite mono runtime!
                ExtractAllSatellites();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Program.RealMain(args);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject.ToString());
            File.WriteAllText("error.log", e.ExceptionObject.ToString());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;

            foreach (var item in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (item.Contains(name))
                {
                    using (var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(item))
                    {
                        if (IsRunningOnMono)
                        {
                            var fs = File.OpenWrite(item);
                            assemblyStream.CopyTo(fs);
                            fs.Flush();
                            fs.Close();
                        }
                        else
                        {
                            byte[] assemblyBuffer = new byte[assemblyStream.Length];
                            assemblyStream.Read(assemblyBuffer, 0, (int)assemblyStream.Length);
                            return Assembly.Load(assemblyBuffer);
                        }
                    }
                }
            }
            if (File.Exists(name+".dll"))
                return Assembly.LoadFile(new FileInfo( name+".dll").FullName);
            return null;
        }

        public static void ExtractAllSatellites()
        {
            var locdir = Path.GetDirectoryName (Assembly.GetExecutingAssembly().Location);

            foreach (var item in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (item.Contains(".dll") && !File.Exists(Path.Combine(locdir, item)))
                {
                    using (var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(item))
                    {
                        byte[] assemblyBuffer = new byte[assemblyStream.Length];
                        var fs = File.OpenWrite(item);
                        assemblyStream.CopyTo(fs);
                        fs.Flush();
                        fs.Close();
                    }
                }
            }
        }
    }
}
