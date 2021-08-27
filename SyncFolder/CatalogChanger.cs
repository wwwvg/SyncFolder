



                            //НЕ ИСПОЛЬЗУЕТСЯ. ОСТАВЛЕН НА ПАМЯТЬ.


using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = @"C:\WINDOWS\system32\xcopy.exe";
            proc.StartInfo.Arguments = $"{originFolder} {destinFolder} /E /I /H /R /Y";
            // /E = скопировать подпапки(пустые в том числе!).
            // /I = если дестинейшн не существует, создать папку с нужным именем. 
            // /H - Копирование, среди прочих, скрытых и системных файлов.
            // /R - Перезапись файлов, предназначенных только для чтения.
            // /Y - Подавление запроса подтверждения на перезапись существующего целевого файла.
             proc.Start();
        }

        internal static void DeleteFile(string destinFolder) //
        {
            FileInfo fileInf = new FileInfo(destinFolder);
            
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }
            /*
            else//если это не файл - значит каталог, хотя в случае с копированием такой подход не работал
            {
                DirectoryInfo dirInfo = new DirectoryInfo(destinFolder);
                if (dirInfo.Exists)
                {
                    dirInfo.Delete(true);
                }
            }
            */
        }

        internal static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Нет такой папки: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
