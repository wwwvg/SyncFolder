using SyncFolder.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace SyncFolder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _BackgroundWorker;
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
                TextBoxDestinationFolder.Text = folderBrowser.SelectedPath.ToString();
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
                return false;
            }
            if (interval <= 0)
            {
                TextBoxStatus.Text = "Некорректный интервал!";
                return false;
            }
            if (TextBoxOriginFolder.Text == TextBoxDestinationFolder.Text)
            {
                TextBoxStatus.Text = "Указана одна и та же папка!";
                return false;
            }

                                                                          //добавить проверку существования папок и файла
                                                                                                    
            return true;
        }
        #endregion

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ButtonStop.IsEnabled = false;
            ButtonStart.IsEnabled = true;
            _BackgroundWorker.CancelAsync();                //остановка BackgroundWorker
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckInputValues()) return;

            ListOfChanges.Items.Clear();
            InputParams inputParams = new InputParams(TextBoxOriginFolder.Text, TextBoxDestinationFolder.Text, TextBoxLogFile.Text, Int32.Parse(TextBoxInterval.Text));
            _BackgroundWorker.RunWorkerAsync(inputParams);                 //запуск BackgroundWorker

            // if()
            TextBoxStatus.Text = "Синхронизация начата!";
            ProgressBarChanges.Visibility = Visibility.Visible;
            ButtonStop.IsEnabled = true;
            ButtonStart.IsEnabled = false;
        }
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            InputParams input = (InputParams)e.Argument;
            IEnumerable<string> list = DifferenceFinder.Find(input.OriginFolder, input.DestinationFolder, input.LogFileName, input.Interval, _BackgroundWorker);
            if (_BackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            e.Result = list;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                TextBoxStatus.Text = "Синхронизация прервана!";
            }
            else if(e.Error != null)
            {
                TextBoxStatus.Text = e.Error.Message;
            }
            else
            {
                IEnumerable<string> list = (IEnumerable<string>)e.Result;
                foreach (var item in list)
                {
                    ListOfChanges.Items.Insert(0, item);
                }
                TextBoxStatus.Text = "Синхронизация выполнена!";
            }
            ButtonStop.IsEnabled = false;
            ButtonStart.IsEnabled = true;
            ProgressBarChanges.Value = 0;
            ProgressBarChanges.Visibility = Visibility.Hidden;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarChanges.Value = e.ProgressPercentage;
        }
    }
}
