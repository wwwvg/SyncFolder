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
        public static List<string> Find(string originFolder, string destinationFolder, string logFileName, int interval, BackgroundWorker backgroundWorker)
        {
            _BackgroundWorker = backgroundWorker;
            List<string> list1 = GetRecursFiles(originFolder);
            List<string> list2 = GetRecursFiles(originFolder); //ЗАМЕНИТЬ
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
            return list1;
        }

        private static List<string> GetRecursFiles(string start_path)
        {
            List<string> ls = new List<string>();
            try
            {
                string[] folders = Directory.GetDirectories(start_path);
                foreach (string folder in folders)
                {
                    ls.Add("Папка: " + folder);
                    ls.AddRange(GetRecursFiles(folder));
                }
                string[] files = Directory.GetFiles(start_path);
                foreach (string filename in files)
                {
                    ls.Add("Файл: " + filename);
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
