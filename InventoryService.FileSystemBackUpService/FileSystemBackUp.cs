using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryService.BackUpService;

namespace InventoryService.FileSystemBackUpService
{
    public class FileSystemBackUp : IBackUpService
    {
        private string BackUpDirectory { set; get; }

        public FileSystemBackUp(string backUpDirectory)
        {
            if (string.IsNullOrEmpty(backUpDirectory)) throw new ArgumentNullException(nameof(backUpDirectory));
            BackUpDirectory = backUpDirectory.TrimEnd('/').TrimEnd('\\');
            if (!System.IO.Directory.Exists(BackUpDirectory))
            {
                System.IO.Directory.CreateDirectory(BackUpDirectory);
            }
        }

        public bool BackUp(string name, string content)
        {
            try
            {
                if (content == null) throw new ArgumentNullException(nameof(content));
                var fileName = Path.Combine(BackUpDirectory, name);
                System.IO.File.WriteAllText(fileName, content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
    
        }
    }
}
