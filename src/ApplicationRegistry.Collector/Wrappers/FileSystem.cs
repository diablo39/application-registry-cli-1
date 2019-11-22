using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegistry.Collector.Wrappers
{
    /// <summary>
    /// Thread safe
    /// </summary>
    class FileSystem
    {
        public Task File_WriteAllTextAsync(string path, string contents, Encoding encoding)
        {
            return File.WriteAllTextAsync(path, contents, encoding);
        }
    }
}
