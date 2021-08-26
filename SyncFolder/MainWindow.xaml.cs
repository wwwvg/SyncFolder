using SyncFolder.Properties;
using System;
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
        public BackgroundWorker _BackgroundWorker { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            _BackgroundWorker = (BackgroundWorker)FindResource("BackgroundWorker");
        }

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
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            
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

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
    }
}
