using System.IO;
using System.Text;
using Mono.Cecil;

namespace Controller
{
    public static class BootstrapCreator
    {
        public static byte[] CreateCustomBootstrap(string project, string endpoint)
        {
            var definition = AssemblyDefinition.ReadAssembly(typeof (Bootstrap.Program).Assembly.Location);
            definition.MainModule.Resources.RemoveAt(0);
            definition.MainModule.Resources.RemoveAt(0);
            var temp = new EmbeddedResource("Bootstrap.endpoint.txt", ManifestResourceAttributes.Public,
                Encoding.ASCII.GetBytes(endpoint));
            definition.MainModule.Resources.Add(temp);
            temp = new EmbeddedResource("Bootstrap.project.txt", ManifestResourceAttributes.Public,
                Encoding.ASCII.GetBytes(project));
            definition.MainModule.Resources.Add(temp);
            
            foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.dll"))
            {
                temp = new EmbeddedResource(file.Name, ManifestResourceAttributes.Public, File.ReadAllBytes(file.Name));
                definition.MainModule.Resources.Add(temp);
            }

            foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.pdb"))
            {
                temp = new EmbeddedResource(file.Name, ManifestResourceAttributes.Public, File.ReadAllBytes(file.Name));
                definition.MainModule.Resources.Add(temp);
            }

            foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.dll.config"))
            {
                temp = new EmbeddedResource(file.Name, ManifestResourceAttributes.Public, File.ReadAllBytes(file.Name));
                definition.MainModule.Resources.Add(temp);
            }

            using (var memory = new MemoryStream())
            {
                definition.Write(memory);
                var bytes = new byte[memory.Position];
                memory.Seek(0, SeekOrigin.Begin);
                memory.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }
    }
}