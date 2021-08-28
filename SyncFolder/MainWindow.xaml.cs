using SyncFolder.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace SyncFolder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _BackgroundWorker;
        private BackgroundWorker _BackgroundWorker2;
        private IList<Data> _Datas = new ObservableCollection<Data>();
        private Process _Process1;
        private Process _Process2;
        public MainWindow()
        {
            InitializeComponent();
            _BackgroundWorker = (BackgroundWorker)FindResource("BackgroundWorker");
            ButtonStop.IsEnabled = false;
            ProgressBarChanges.Visibility = Visibility.Hidden;
            ListOfChanges.ItemsSource = _Datas;
            Settings.Default.PropertyChanged += Default_PropertyChanged;
        }
        private void BackgroundWorkerInit()
        {
            _BackgroundWorker2.WorkerReportsProgress = true;
            _BackgroundWorker2.WorkerSupportsCancellation = true;
            _BackgroundWorker2.DoWork += BackgroundWorker_DoWork;
            _BackgroundWorker2.ProgressChanged += BackgroundWorker_ProgressChanged;
            _BackgroundWorker2.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

        }
        #region ОТКРЫТИЕ ПАПОК
        private void OpenOriginFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.SelectedPath = TextBoxOriginFolder.Text;
            DialogResult result = folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                TextBoxOriginFolder.Text = folderBrowser.SelectedPath.ToString();
        }

        private void OpenDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.SelectedPath = TextBoxDestinFolder.Text;
            DialogResult result = folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                TextBoxDestinFolder.Text = folderBrowser.SelectedPath.ToString();
        }

        private void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog
                    dlg = new Microsoft.Win32.OpenFileDialog(); //диалоговое окно выбора файла
                dlg.InitialDirectory = TextBoxLogFile.Text;
                var result = dlg.ShowDialog();
                if (result == true)
                    TextBoxLogFile.Text = dlg.FileName;
            }
            catch(Exception){}//
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

            if (!File.Exists(TextBoxLogFile.Text))
            {
                TextBoxStatus.Text = "Файла не существует!";
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
                return false;
            }
            return true;
        }
        #endregion

        private void ButtonStart_Click(object sender, RoutedEventArgs e)        //START
        {
            if (!CheckInputValues()) return;        //проверка входных значений

            InputParams inputParams = new InputParams(TextBoxOriginFolder.Text, TextBoxDestinFolder.Text, TextBoxLogFile.Text, Int32.Parse(TextBoxInterval.Text));
            try
            {
                _BackgroundWorker2 = new BackgroundWorker();
                BackgroundWorkerInit();
                _BackgroundWorker2.RunWorkerAsync(inputParams); //==================================ЗАПУСК BackgroundWorker=====================================
                //_BackgroundWorker.RunWorkerAsync(inputParams); //==================================ЗАПУСК BackgroundWorker=====================================
            }
            catch (System.InvalidOperationException)//вылетает <<System.InvalidOperationException>> если быстро херачить туда-сюда старт/стоп
            {
                //MessageBox.Show(ex.Message);
                return;
            }

            TextBoxStatus.Text = "Выполняется синхронизация";
            TextBoxStatus.ToolTip = TextBoxStatus.Text;
            ButtonStop.IsEnabled = true;
            ButtonStart.IsEnabled = false;
            UnlockButtons(false);
            
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)//НАЧАЛО РАБОТЫ
        {
            DateTime startTime = DateTime.Now;
            int k = 1;
            while (true)
            {
                k++;
                InputParams input = (InputParams) e.Argument;
                DifferenceFinder.Find(input.OriginFolder, input.DestinationFolder, input.LogFileName, 
                    _BackgroundWorker2, ListOfChanges, _Datas, TextBoxStatus); //РЕЗУЛЬТАТ

                if (_BackgroundWorker2.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(input.Interval); //установка значения задаваемого пользователем!

                if ((input.Interval * k) % 100 == 0)
                {
                    TextBoxStatus.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart) delegate()
                        {
                            if (TextBoxStatus.Text.Contains("Выполняется синхронизация"))
                            {
                                TextBoxStatus.Text += ".";
                                TextBoxStatus.Text = TextBoxStatus.Text.Replace("..........", ".");
                            }
                        }
                    );
                }
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)  //КОНЕЦ РАБОТЫ
        {
            if (e.Cancelled)
            {
                TextBoxStatus.Text = "Синхронизация остановлена.";
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
            }
            else if(e.Error != null)
            {
                TextBoxStatus.Text = e.Error.Message;
                TextBoxStatus.ToolTip = TextBoxStatus.Text;
            }

            ButtonStop.IsEnabled = false;
            ButtonStart.IsEnabled = true;
            UnlockButtons(true);
            ProgressBarChanges.Value = 0;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)        //ПРОГРЕСС РАБОТЫ
        {
            ProgressBarChanges.Value = e.ProgressPercentage;
            if((bool)e.UserState == false)
                ProgressBarChanges.Visibility = Visibility.Hidden;
            else
            {
                ProgressBarChanges.Visibility = Visibility.Visible;
            }
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

        private void UnlockButtons(bool isEnabled)
        {
            TextBoxInterval.IsEnabled = isEnabled;
            TextBoxLogFile.IsEnabled = isEnabled;
            TextBoxOriginFolder.IsEnabled = isEnabled;
            TextBoxDestinFolder.IsEnabled = isEnabled;
            OpenOriginFolder.IsEnabled = isEnabled;
            OpenDestinationFolder.IsEnabled = isEnabled;
            OpenLogFile.IsEnabled = isEnabled;
        }

        private void ImageClose_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Settings.Default.Save();
            System.Windows.Application.Current.Shutdown();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ButtonStop.IsEnabled = false;
            ButtonStart.IsEnabled = true;
            _BackgroundWorker2.CancelAsync();                //остановка BackgroundWorker
            UnlockButtons(true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void ButtonClearList_OnClick(object sender, RoutedEventArgs e)
        {
            _Datas.Clear();
        }
    }
}
