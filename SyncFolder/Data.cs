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

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Id"));
            }
        }

        private string _typeOfAction;
        public string TypeOfAction
        {
            get { return _typeOfAction; }
            set
            {
                _typeOfAction = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TypeOfAction"));
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
