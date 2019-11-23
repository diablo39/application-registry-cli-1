using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Wrappers
{
    /// <summary>
    /// Thread safe
    /// </summary>
    internal class FileSystem
    {
        public virtual Task WriteAllTextAsync(string path, string contents, Encoding encoding)
        {
            return File.WriteAllTextAsync(path, contents, encoding);
        }
    }
}
