using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SyncFolder
{
    class DifferenceFinder
    {
        private static BackgroundWorker _BackgroundWorker;
        public static IEnumerable<string> Find(string originFolder, string destinFolder, string logFileName, int interval, BackgroundWorker backgroundWorker)
        {
            _BackgroundWorker = backgroundWorker;
            List<string> list1 = GetRecursFiles(originFolder);
            List<string> list2 = GetRecursFiles(destinFolder);
            
            //ищем новые файлы в 1м каталоге и добавляем их во 2й
            IEnumerable<string> query = from file in list1.Except(list2) orderby file descending select originFolder + "\\" + file; 
            foreach (var item in query)
            {
                CatalogChanger.AddFile(item, destinFolder + item.Replace(originFolder, ""));
            }
            var listOfAdded = query.ToList();

            for (int i = 0; i < listOfAdded.Count(); i++)
            {
                listOfAdded[i] = "[+] " + listOfAdded[i];
            }

            //ищем старые файлы во 2м каталоге и удаляем их
            query = from file in list2.Except(list1) orderby file descending select destinFolder + "\\" + file;
            foreach (var item in query)
            {
                CatalogChanger.DeleteFile(item);
            }
            var listOfDeleted = query.ToList();

            for (int i = 0; i < listOfDeleted.Count(); i++)
            {
                listOfDeleted[i] = "[-] " + listOfDeleted[i];
            }

            listOfAdded.AddRange(listOfDeleted); //список добавленных и удаленных

            //Обновляем списки каталогов. Структура каталогов должна быть одинакова.
            list1 = GetRecursFiles(originFolder);
            list2 = GetRecursFiles(destinFolder);
            List<string> listOfChanged = new List<string>(); //список неравных файлов
            try
            {
                for (int i = 0; i < list1.Count; i++)
                {
                    if (!FileCompare(originFolder + "\\" + list1[i], destinFolder + "\\" + list2[i]))
                    {
                        listOfChanged.Add("[%] " + list1[i]);
                        CatalogChanger.AddFile(originFolder + "\\" + list1[i], destinFolder + "\\" + list1[i].Replace(originFolder, ""));
                    }                  
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }

            listOfAdded.AddRange(listOfChanged);

            return listOfAdded;
        }

        private static List<string> GetRecursFiles(string start_path)
        {
            List<string> ls = new List<string>();
            try
            {
                string[] folders = Directory.GetDirectories(start_path);
                foreach (string folder in folders)
                {
                    ls.Add(/*"Папка: " + */folder.Replace(start_path + "\\", ""));
                    ls.AddRange(GetRecursFiles(folder));
                }
                string[] files = Directory.GetFiles(start_path);
                foreach (string filename in files)
                {
                    ls.Add(/*"Файл: " + */filename.Replace(start_path + "\\", ""));
                }
            }
            catch (System.Exception e)
            {
                //MessageBox.Show(e.Message);
            }
            return ls;
        }

        #region //Использование visual C# для создания File-Compare функции
        //https://docs.microsoft.com/ru-ru/troubleshoot/dotnet/csharp/create-file-compare
        private static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            // Check the file sizes. If they are not the same, the files
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is
            // equal to "file2byte" at this point only if the files are
            // the same.
            return ((file1byte - file2byte) == 0);
        }
        #endregion
    }


}
/*
 * 1. Привести в соотв. структуру каталогов и файлов
 *      1й объединить со 2м 
 *      1й пересечь с пред. результатом
 * 2. Проверить каждый файл побитово
 */


/*
            int iteration = list1.Count / 100;
            
            for (int i = 0; i < list1.Count; i++)
            {
                if ((i % iteration == 0) && (_BackgroundWorker != null) && _BackgroundWorker.WorkerReportsProgress)
                {
                    _BackgroundWorker.ReportProgress(i / iteration);
                }
                for (int j = 0; j < list2.Count; j++)
                {                                              
                    if (_BackgroundWorker.CancellationPending)
                    {
                        return new List<string>();
                    }
                   // Thread.Sleep(10);
                }
            }
            */

//var temp = from file in list1.Union(list2) orderby file descending select file;
//var query = from file in list1.Intersect(temp) orderby file descending select file;