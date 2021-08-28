using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SyncFolder
{
    class DifferenceFinder
    {
        private static BackgroundWorker _BackgroundWorker;
        private static ListView _ListView;
        private static TextBlock _TextBoxStatusBox;
        private static IList<Data> _Datas;
        private static string _LogFileName;

        private const string _sFOLDER = "Папка";
        private const string _sFILE = "Файл";
        private const string _sADDED_ICON =   "\\Icons\\added.png";
        private const string _sDELETED_ICON = "\\Icons\\deleted.png";
        private const string _sUPDATED_ICON = "\\Icons\\update.png";
        private const string _sFILE_ICON =    "\\Icons\\file.png";
        private const string _sFOLDER_ICON =  "\\Icons\\folder.png";
        
        public static IList<Data> Find(string originFolder, string destinFolder, string logFileName, 
                                            BackgroundWorker backgroundWorker, ListView listView, IList<Data> datas, TextBlock textBoxStatusBox)
        {
            _BackgroundWorker = backgroundWorker;
            _ListView = listView;
            _Datas = datas;
            _LogFileName = logFileName;
            _TextBoxStatusBox = textBoxStatusBox;
            List<string> list1 = GetRecursFiles(originFolder);
            List<string> list2 = GetRecursFiles(destinFolder);
            List<string> listFolders1 = new List<string>();
            List<string> listFiles1 = new List<string>();
            List<string> listFolders2 = new List<string>();
            List<string> listFiles2 = new List<string>();

            //string sDateTimeNow = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
//ДОБАВЛЕНИЕ УНИКАЛЬНЫХ ПАПОК ИЗ ИСТОЧНИКА

            listFolders1 = (from file in list1 where file.Contains(_sFOLDER) select file.Replace(_sFOLDER + originFolder, "")).ToList(); //папки в источнике
            listFolders2 = (from file in list2 where file.Contains(_sFOLDER) select file.Replace(_sFOLDER + destinFolder, "")).ToList(); //папки в приемнике

            //ищем новые папки в 1м каталоге и добавляем их во 2й
            var query = from folder in listFolders1.Except(listFolders2) select destinFolder + folder;
            int x = 0;
            foreach (var folder in query)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folder);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                    var data = new Data { TypeOfAction = _sADDED_ICON, TypeOfFile = _sFOLDER_ICON, TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = folder};
                    UpdateListView(data);
                    x++;
                }
            }

//ДОБАВЛЕНИЕ УНИКАЛЬНЫХ ФАЙЛОВ ИЗ ИСТОЧНИКА

            listFiles1 = (from file in list1 where file.Contains(_sFILE) select file.Replace(_sFILE + originFolder, "")).ToList(); //файлы в источнике
            listFiles2 = (from file in list2 where file.Contains(_sFILE) select file.Replace(_sFILE + destinFolder, "")).ToList(); //файлы в приемнике

            //ищем новые файлы в 1м каталоге и добавляем их во 2й
            query = from file in listFiles1.Except(listFiles2) select destinFolder + file;
            int maxRecords = Math.Max(listFolders1.Count, listFolders2.Count) + Math.Max(listFiles1.Count, listFiles2.Count);
            
            foreach (var file in query)
            {
                FileInfo fileInf = new FileInfo(file.Replace(destinFolder, originFolder));
                if (fileInf.Exists)
                {
                    fileInf.CopyTo(file);
                    var data = new Data { TypeOfAction = _sADDED_ICON, TypeOfFile = _sFILE_ICON, TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = file };
                    UpdateListView(data , x, maxRecords);
                    x++;
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
                    var data = new Data { TypeOfAction = _sDELETED_ICON, TypeOfFile = _sFILE_ICON, TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = file };
                    UpdateListView(data, x, maxRecords);
                    x++;
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
                    var data = new Data { TypeOfAction = _sDELETED_ICON, TypeOfFile = _sFOLDER_ICON, TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = folder };
                    UpdateListView(data, x, maxRecords);
                    x++;
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
                    string fileName1 = list1[i].Replace(_sFILE, "");
                    string fileName2 = list2[i].Replace(_sFILE, "");
                    if (list1[i].Contains(_sFOLDER) || list2[i].Contains(_sFOLDER)) continue;

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
                            var data = new Data { TypeOfAction = _sUPDATED_ICON, TypeOfFile = _sFILE_ICON, TimeStamp = DateTime.Now.ToString("HH:mm:ss"), Path = fileName2 };
                            UpdateListView(data, x, maxRecords);
                            x++;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
                return new List<Data>();
            }
            _BackgroundWorker.ReportProgress(0, false);

            return _Datas; //ВОЗВРАЩАЕТСЯ СПИСОК ИЗМЕНЕНИЙ ДЛЯ LISTVIEW
            
        }

        private static void UpdateStatusBox(object sender, EventArgs e)
        {
            _TextBoxStatusBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart) delegate()
                {
                    _TextBoxStatusBox.Text += ".";
                }
            );
        }
        private static void UpdateListView(Data data, int x = 50, int maxRecords = 100) // х - процент изменений
        {
            _BackgroundWorker.ReportProgress(Convert.ToInt32(((decimal)x / (decimal)maxRecords) * 100), true);
            _ListView.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    data.Id = _Datas.Count + 1;
                    _Datas.Add(data);
                    
                    #region ЗАПИСЬ *.CSV
                    using (StreamWriter sw = new StreamWriter(_LogFileName, true, System.Text.Encoding.Default))
                    {
                        string sAction = string.Empty;
                        switch (data.TypeOfAction)
                        {
                            case _sADDED_ICON:
                                sAction = "[+]";
                                break;
                            case _sDELETED_ICON:
                                sAction = "[-]";
                                break;
                            case _sUPDATED_ICON:
                                sAction = "[%]";
                                break;
                        }

                        string sFileType = string.Empty;
                        switch (data.TypeOfFile)
                        {
                            case _sFILE_ICON:
                                sFileType = "File";
                                break;
                            case _sFOLDER_ICON:
                                sFileType = "Folder";
                                break;
                        }

                        string csv = $"{sAction}\t{sFileType}\t{data.TimeStamp}\t{DateTime.Now.ToString("dd/MM/yyyy")}\t{data.Path}\n";
                        sw.Write(csv);
                    }
                    #endregion
                }
            );
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
                    
                    list.Add(_sFOLDER + folder/*.Replace(start_path + "\\", "")*/);
                    list.AddRange(GetRecursFiles(folder));
                   
                }
                string[] files = Directory.GetFiles(start_path);
                foreach (string filename in files)
                {
                    list.Add(_sFILE + filename/*.Replace(start_path + "\\", "")*/);
                }
            }
            catch (System.Exception e)
            {
                throw e;
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


/*
Написать программу, которая будет синхронизировать два каталога: каталог - источник и каталог-реплику. 
Задача программы – приводить содержимое каталога-реплики в соответствие содержимому каталога-источника.
Требования:
•	Сихронизация должна быть односторонней: после завершения процесса синхронизации содержимое каталога-реплики должно в 
точности соответствовать содержимому каталогу-источника;
•	Синхронизация должна производиться периодически;
•	Операции создания/копирования/удаления объектов должны логироваться в файле и выводиться в консоль;
•	Пути к каталогам, интервал синхронизации и путь к файлу логирования должны задаваться параметрами командной строки при запуске программы.
*/