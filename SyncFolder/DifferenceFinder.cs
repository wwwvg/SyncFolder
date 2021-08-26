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
                listOfAdded[i] = "[+ ] " + listOfAdded[i];
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
                listOfDeleted[i] = "[- ] " + listOfDeleted[i];
            }

            listOfAdded.AddRange(listOfDeleted);

            return listOfDeleted;
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