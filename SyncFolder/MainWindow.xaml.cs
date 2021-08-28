using SyncFolder.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace SyncFolder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _BackgroundWorker;
        private List<Data> _Datas;
        private Process _Process1;
        private Process _Process2;
        public MainWindow()
        {
            InitializeComponent();
            _BackgroundWorker = (BackgroundWorker)FindResource("BackgroundWorker");
            
            ButtonStop.IsEnabled = false;
            ProgressBarChanges.Visibility = Visibility.Hidden;
        }
        #region ОТКРЫТИЕ ПАПОК
        private void OpenOriginFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            DialogResult result = folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                TextBoxOriginFolder.Text = folderBrowser.SelectedPath.ToString();
        }

        private void OpenDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            DialogResult result = folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                TextBoxDestinFolder.Text = folderBrowser.SelectedPath.ToString();
        }

        private void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog(); //диалоговое окно выбора файла
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
                TextBoxLogFile.Text = dlg.FileName;     //установил текст в текстовое поле
        }
        #endregion

        #region ПРОВЕРКА ВХОДНЫХ ДАННЫХ
        
        private bool CheckInputValues()
        {
            if (!Int32.TryParse(TextBoxInterval.Text, out int interval))
            {
                TextBoxStatus.Text = "Некорректный интервал!";
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
                return false;
            }
            if (interval <= 0)
            {
                TextBoxStatus.Text = "Некорректный интервал!";
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
                return false;
            }
            if (TextBoxOriginFolder.Text == TextBoxDestinFolder.Text)
            {
                TextBoxStatus.Text = "Указана одна и та же папка!";
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
                return false;
            }

                                                                      
                                                                                                    
            return true;
        }
        #endregion

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ButtonStop.IsEnabled = false;
            ButtonStart.IsEnabled = true;
            _BackgroundWorker.CancelAsync();                //остановка BackgroundWorker
        }

       

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckInputValues()) return;

            InputParams inputParams = new InputParams(TextBoxOriginFolder.Text, TextBoxDestinFolder.Text, TextBoxLogFile.Text, Int32.Parse(TextBoxInterval.Text));
            _BackgroundWorker.RunWorkerAsync(inputParams);                 //запуск BackgroundWorker

            // if()
            TextBoxStatus.Text = "Синхронизация начата!";
            TextBoxStatus.ToolTip = TextBoxStatus.Text;
            ProgressBarChanges.Visibility = Visibility.Visible;
            ButtonStop.IsEnabled = true;
            ButtonStart.IsEnabled = false;

        }
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)//НАЧАЛО РАБОТЫ
        {           
            InputParams input = (InputParams)e.Argument;

            _Datas = new List<Data>();
            _Datas.AddRange(DifferenceFinder.Find(input.OriginFolder, input.DestinationFolder, input.LogFileName, input.Interval, _BackgroundWorker));//РЕЗУЛЬТАТ
            setId(_Datas);

            if (_BackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            
            e.Result = _Datas;//РЕЗУЛЬТАТ
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)  //КОНЕЦ РАБОТЫ
        {
            if (e.Cancelled)
            {
                TextBoxStatus.Text = "Синхронизация прервана!";
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
            }
            else if(e.Error != null)
            {
                TextBoxStatus.Text = e.Error.Message;
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
            }
            else
            {
               // List<Data> _Datas = (List<Data>)e.Result;//РЕЗУЛЬТАТ
               // setId(_Datas);
                ListOfChanges.ItemsSource = (List<Data>)e.Result;//РЕЗУЛЬТАТ;

                TextBoxStatus.Text = "Синхронизация выполнена!";
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
            }
            ButtonStop.IsEnabled = false;
            ButtonStart.IsEnabled = true;
            ProgressBarChanges.Value = 0;
            ProgressBarChanges.Visibility = Visibility.Hidden;
        }

        private void setId(List<Data> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                datas[i].Id = (i + 1).ToString();
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)        //ПРОГРЕСС РАБОТЫ
        {
            ProgressBarChanges.Value = e.ProgressPercentage;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ButtonExplorer_OnClick(object sender, RoutedEventArgs e)
        {
            _Process1 = Process.Start("explorer.exe", TextBoxOriginFolder.Text);
            _Process2 = Process.Start("explorer.exe", TextBoxDestinFolder.Text);
        }
    }
}
