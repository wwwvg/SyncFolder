using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncFolder
{
    class Data : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private string id;
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Id"));
            }
        }

        private string imagePath;
        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                imagePath = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ImagePath"));
            }
        }

        private string typeOfFile;
        public string TypeOfFile
        {
            get { return typeOfFile; }
            set
            {
                typeOfFile = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TypeOfFile"));
            }
        }

        private string timeStamp;
        public string TimeStamp
        {
            get { return timeStamp; }
            set
            {
                timeStamp = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TimeStamp"));
            }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Path"));
            }
        }
    }
}
