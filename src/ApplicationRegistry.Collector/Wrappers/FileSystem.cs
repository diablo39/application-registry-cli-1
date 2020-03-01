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
        public virtual string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
        

    public virtual Task WriteAllTextAsync(string path, string contents, Encoding encoding)
        {
            return File.WriteAllTextAsync(path, contents, encoding);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}
