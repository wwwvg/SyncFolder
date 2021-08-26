using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncFolder
{
    class InputParams
    {
        public string OriginFolder { get; set; }
        public string DestinationFolder { get; set; }
        public string LogFileName { get; set; }
        public int Interval { get; set; }

        public InputParams(string originFolder, string destinationFolder, string logFileName, int interval)
        {
            OriginFolder = originFolder;
            DestinationFolder = destinationFolder;
            LogFileName = logFileName;
            Interval = interval;
        }
    }
}
