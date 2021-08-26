using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncFolder
{
    class CatalogChanger
    {
        internal static void AddFile(string originFolder, string destinFolder)
        {
            FileInfo fileInf = new FileInfo(originFolder);
            if (fileInf.Exists)
            {
                fileInf.CopyTo(destinFolder, true);
                // альтернатива с помощью класса File
                // File.Copy(path, newPath, true);
            }
        }

        internal static void DeleteFile(string destinFolder)
        {
            FileInfo fileInf = new FileInfo(destinFolder);
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }
        }
    }
}
