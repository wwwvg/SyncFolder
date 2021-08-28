using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        public static IList<Data> _Datas = new List<Data>();

        public static IList<Data> Find(string originFolder, string destinFolder, string logFileName, int interval, BackgroundWorker backgroundWorker)
        {
            _BackgroundWorker = backgroundWorker;
            List<string> list1 = GetRecursFiles(originFolder);
            List<string> list2 = GetRecursFiles(destinFolder);
            List<string> listFolders1 = new List<string>();
            List<string> listFiles1 = new List<string>();
            List<string> listFolders2 = new List<string>();
            List<string> listFiles2 = new List<string>();


//ДОБАВЛЕНИЕ УНИКАЛЬНЫХ ПАПОК ИЗ ИСТОЧНИКА

            listFolders1 = (from file in list1 where file.Contains("Папка") select file.Replace("Папка" + originFolder, "")).ToList(); //папки в источнике
            listFolders2 = (from file in list2 where file.Contains("Папка") select file.Replace("Папка" + destinFolder, "")).ToList(); //папки в приемнике

            //ищем новые папки в 1м каталоге и добавляем их во 2й
            var query = from folder in listFolders1.Except(listFolders2) select destinFolder + folder;
            int ii = 0;
            foreach (var folder in query)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folder);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                    _Datas.Add(new Data { ImagePath = @"\Icons\added.png", TypeOfFile = @"\Icons\folder.png", TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = folder});
                   // if(ii < 20) _BackgroundWorker.ReportProgress(ii++);
                }
            }

//ДОБАВЛЕНИЕ УНИКАЛЬНЫХ ФАЙЛОВ ИЗ ИСТОЧНИКА

            listFiles1 = (from file in list1 where file.Contains("Файл") select file.Replace("Файл" + originFolder, "")).ToList(); //файлы в источнике
            listFiles2 = (from file in list2 where file.Contains("Файл") select file.Replace("Файл" + destinFolder, "")).ToList(); //файлы в приемнике

            //ищем новые файлы в 1м каталоге и добавляем их во 2й
            query = from file in listFiles1.Except(listFiles2) select destinFolder + file;

            foreach (var file in query)
            {
                FileInfo fileInf = new FileInfo(file.Replace(destinFolder, originFolder));
                if (fileInf.Exists)
                {
                    fileInf.CopyTo(file);
                    _Datas.Add(new Data { ImagePath = @"\Icons\added.png", TypeOfFile = @"\Icons\file.png", TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = file });
                   // if (ii < 40) _BackgroundWorker.ReportProgress(ii++);
                }

            }

//УДАЛЕНИЕ УНИКАЛЬНЫХ ФАЙЛОВ ИЗ КЛОНА

            //ищем уникальные файлы во 2м каталоге и удаляем их
            query = from file in listFiles2.Except(listFiles1) select destinFolder + file;

            foreach (var file in query)
            {
                FileInfo fileInf = new FileInfo(file);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                    _Datas.Add(new Data { ImagePath = @"\Icons\deleted.png", TypeOfFile = @"\Icons\file.png", TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = file });
                   // if (ii < 60) _BackgroundWorker.ReportProgress(ii++);
                }
            }

//УДАЛЕНИЕ УНИКАЛЬНЫХ ПАПОК ИЗ КЛОНА
            
            //ищем уникальные папки во 2м каталоге и удаляем их
            query = from folder in listFolders2.Except(listFolders1) select destinFolder + folder;

            foreach (var folder in query)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folder);
                if (dirInfo.Exists)
                {
                    dirInfo.Delete(true);
                    _Datas.Add(new Data { ImagePath = @"\Icons\deleted.png", TypeOfFile = @"\Icons\folder.png", TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = folder });
                    //if (ii < 80) _BackgroundWorker.ReportProgress(ii++);
                }
            }
                //СРАВНЕНИЕ СОДЕРЖИМОГО ФАЙЛОВ

            list1 = GetRecursFiles(originFolder);
            list2 = GetRecursFiles(destinFolder);

            try
            {
                for (int i = 0; i < list1.Count; i++)
                {
                    if (list1.Count != list2.Count) break;
                    string fileName1 = list1[i].Replace("Файл", "");
                    string fileName2 = list2[i].Replace("Файл", "");
                    if (list1[i].Contains("Папка") || list2[i].Contains("Папка")) continue;

                    if (fileName1.Replace(originFolder,"") == fileName2.Replace(destinFolder,""))
                    {
                        if (!FileCompare(fileName1, fileName2))
                        {
                            FileInfo fileInf = new FileInfo(fileName2);
                            if (fileInf.Exists)
                                fileInf.Delete();

                            fileInf = new FileInfo(fileName1);
                            if (fileInf.Exists)
                                fileInf.CopyTo(fileName2, true);
                            _Datas.Add(new Data { ImagePath = @"\Icons\update.png", TypeOfFile = @"\Icons\file.png", TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = fileName2 });
                         //   if (ii < 90) _BackgroundWorker.ReportProgress(ii++);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
            return _Datas; //ВОЗВРАЩАЕТСЯ СПИСОК ИЗМЕНЕНИЙ ДЛЯ LISTVIEW
            
        }
        #region РЕКУРСИВНЫЙ ПОИСК ФАЙЛОВ И КАТАЛОГОВ
        private static List<string> GetRecursFiles(string start_path)
        {
            List<string> list = new List<string>();
            try
            {
                string[] folders = Directory.GetDirectories(start_path);
                foreach (string folder in folders)
                {
                    
                    list.Add("Папка" + folder/*.Replace(start_path + "\\", "")*/);
                    list.AddRange(GetRecursFiles(folder));
                   
                }
                string[] files = Directory.GetFiles(start_path);
                foreach (string filename in files)
                {
                    list.Add("Файл" + filename/*.Replace(start_path + "\\", "")*/);
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return list;
        }
        #endregion

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
