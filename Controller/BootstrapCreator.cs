using System.IO;
using System.Text;
using Mono.Cecil;

namespace Controller
{
    public static class BootstrapCreator
    {
        public static byte[] CreateCustomBootstrap(string token)
        {
            var definition = AssemblyDefinition.ReadAssembly(typeof (Bootstrap.Program).Assembly.Location);
            var temp = new EmbeddedResource("Bootstrap.token.txt", ManifestResourceAttributes.Public,
                Encoding.ASCII.GetBytes(token));
            definition.MainModule.Resources.RemoveAt(0);
            definition.MainModule.Resources.Add(temp);

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