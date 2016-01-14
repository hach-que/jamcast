using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrap
{
    class Prerun
    {

        internal static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Program.RealMain(args);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
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
                        byte[] assemblyBuffer = new byte[assemblyStream.Length];
                        var fs = File.OpenWrite(item);
                        assemblyStream.CopyTo(fs);
                        fs.Flush();
                        fs.Close();
                    }
                }
            }
            if (File.Exists(name+".dll"))
                return Assembly.LoadFile(new FileInfo( name+".dll").FullName);
            return null;
        }
    }
}
