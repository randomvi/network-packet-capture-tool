using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace NETAPP
{
    class Detective
    {
        // detects events that occurs within the system
        public string Removable()
        {
            // return drive name
            var drives = DriveInfo.GetDrives().Where(drive=>drive.IsReady && drive.DriveType==DriveType.Removable);  
            return drives.ToString();
        }



    }
}
