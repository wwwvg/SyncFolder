using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncFolder
{
    class DifferenceFinder
    {
        private static BackgroundWorker _BackgroundWorker;
        public static List<string> Find(string originFolder, string destinationFolder, string logFileName, int interval, BackgroundWorker backgroundWorker)
        {
            _BackgroundWorker = backgroundWorker;
            List<string> list = new List<string>() { "One", "Two"};
            /*
            if (_BackgroundWorker.CancellationPending)
            {
                return new List<string>() { "One",}; ;
            }
            */
            Thread.Sleep(TimeSpan.FromSeconds(5));
            return list;
        }
    }
}
