using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETAPP
{
    class IPHANDLER
    {
        private string ip;

        public string IPADDRESS
        {
            get { return ip; }
            set { ip = value; }
        }
        private string pcname;

        public string ComputerName
        {
            get { return pcname; }
            set { pcname = value; }
        }
        
        public IPHANDLER(string ip,string pname)
        {
            this.ip = ip;
            this.pcname = pname;
        }
    }
}
