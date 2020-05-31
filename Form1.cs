using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net.Security;
using System.Net;
using System.Net.NetworkInformation;
using System.Speech;
using System.Speech.Synthesis;
using SharpPcap;
//using SharpPcap.WinPcap;
//using SharpPcap.LibPcap;
//using SharpPcap.AirPcap;
//using PacketDotNet.Tcp;
using PacketDotNet;
namespace NETAPP
{
    public partial class netclass : Form
    {
        public netclass()
        {
            InitializeComponent();
        }

        // ADAPTER AND SCANNING PROPETIES
        Thread captureThread = null;

        List<string> listOfAdapterDevices = new List<string>();
        SharpPcap.DeviceMode deviceMode;
        ICaptureDevice device;
        EthernetPacket packetToSend;
        private string Filterprotocol = "";
        private string FilterIp = "";
        private List<string> MACnIPs = new List<string>();

        //*************************************************

        List<string> IPS = new List<string>();
        List<string> PCNAME = new List<string>();
        List<IPHANDLER> displaysource = new List<IPHANDLER>();
        List<string> ALLINFO = new List<string>();
        private void netclass_Load(object sender, EventArgs e)
        {
            cmbCaptureMode.DataSource = Enum.GetValues(typeof(SharpPcap.DeviceMode));
            cmbPacketType.DataSource = Enum.GetValues(typeof(EthernetPacketType));
            AutoComplete();// auto complete ips
        }

        public void GetAllIps()
        {
            string halfIP = GetHalfIP();
            Ping pinger = new Ping();
           // txtlocalip.Text = ip1;
            string mask = "255.255.255.0";
           // txtsubnetmask.Text = mask;
            for (int i = 1; i <255; i++)
            {
                string fullIP = halfIP + i;
                IPAddress ipx=IPAddress.Parse(fullIP);
                PingReply Pr = pinger.Send(ipx,600);
                if (Pr.Status == IPStatus.Success)
                {
                    string hostname = "";
                    IPS.Add(fullIP);
                    try
                    {
                        IPHostEntry host = Dns.GetHostEntry(fullIP);
                        hostname = host.HostName;
                    }
                    catch (Exception v)
                    {
                        Console.WriteLine(v.Message);
                    }
                    ALLINFO.Add(fullIP+"#"+hostname);
                }
            }
        }
        public void PutDisplay()
        {
            foreach (var item in ALLINFO)
            {
                string[] infoip=item.Split('#');
                displaysource.Add(new IPHANDLER(infoip[0],infoip[1]));
            }

            this.Invoke(new MethodInvoker(delegate()
            {
                dataGridView1.DataSource = displaysource;
                txtpingip.DataBindings.Add("TEXT", displaysource, "IPADDRESS");
            }));
        }

        public void ThreadCaller()
        {
            GetAllIps();
            PutDisplay();
            System19.Say("You have " + ALLINFO.Count + " Computers connected to you network");
        }

        private void buttonrefreships_Click(object sender, EventArgs e)
        {
            Thread worker = new Thread(new ThreadStart(ThreadCaller));
            worker.Start();
        }

        bool ipstopper = true;
        private void buttonping_Click(object sender, EventArgs e)
        {

            IPAddress ipaddress=IPAddress.Parse(txtpingip.Text);
            Ping pinger = new Ping();

           // int bytes=int.Parse(txtbytes.Text);
          //  byte[] data = new byte[bytes];
            bool getcheck = false;
            if (cbinfinityping.Checked == true)
            {
                for (int i = 0; i < 100000; i++)
                {
                    PingReply Pr = pinger.Send(ipaddress);
                    if (Pr.Status == IPStatus.Success)
                    {
                        getcheck = true;
                    }
                    txtpings.Text = i + "";
                }
            }
            else
            {
                for (int i = 0; i <4; i++)
                {
                    PingReply Pr = pinger.Send(ipaddress,1000);
                    if (Pr.Status == IPStatus.Success)
                    {
                       getcheck = true;
                    }
                }
            }

            if (getcheck == true)
            {

                txtfailedpings.Text = "0";
                txtpings.Text = "4";
                txtsuccesspings.Text = "4";
                SpeechSynthesizer synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                synth.Speak("Computer is online");
                Console.WriteLine();
            }
            else
            {

                txtfailedpings.Text = "ALL";
                txtpings.Text = "4";
                txtsuccesspings.Text = "0";
                SpeechSynthesizer synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                synth.Speak("Computer is not Online");
                Console.WriteLine();
            }
        }

        private void buttonstopping_Click(object sender, EventArgs e)
        {
            ipstopper = false;
        }

        private void btndownload_Click(object sender, EventArgs e)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            synth.Speak("hey stop doing that lol");
            Console.WriteLine();
        }

        //*****************************MOD LAN************************************************
        //checking for open ports on the pc

        private void btnScanPorts_Click(object sender, EventArgs e)
        {
            listOfAdapterDevices = null;
            listOfAdapterDevices = Monitor.GetListOfNetAdaptDevices();
            lstAdapters.DataSource = listOfAdapterDevices;
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            // Captures those packets
            OpenNcapturePackets(device,deviceMode,1000);
        }

        public void SetAdapterName()
        {
            var devices = CaptureDeviceList.Instance;
            foreach (ICaptureDevice dev in devices)
            {
                if (dev.Description == lstAdapters.SelectedItem.ToString())
                {
                    device = dev;
                }
            }
        }

        public void OpenNcapturePackets(ICaptureDevice device, DeviceMode captureMode, int timeOutMillis)
        {
            device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
            // open the device for capturing
            device.Open(captureMode, timeOutMillis);// read all packets on network # if normal read Current Pcs Packets only
            System19.Say("I am Going to use device {0} to Extract Packets ", device.Description);
            device.StartCapture();// starts capturing packets
            //device.StopCapture();

        }

        public string PacketDetails(CaptureEventArgs w)
        {
            string data = "1: ";
            var packet = Packet.ParsePacket(w.Packet.LinkLayerType, w.Packet.Data);
            //getMACAddressAssociated with
            if (packet.ToString().Contains(GetHalfIP()))
            {
                // add the mac and IP to
                MACnIPs.Add(packet.ToString());
            }

            if (packet != null)
            {
                if (packet.ToString().ToUpper().Contains(Filterprotocol) || packet.ToString().ToUpper().Contains(FilterIp))
                {
                    string[] sub = packet.ToString().Split(',');
                    data = sub[2] + "#" + sub[3];
                }
            }
            return data;
        }

        private void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            DateTime time = e.Packet.Timeval.Date;
            int packetLength = e.Packet.Data.Length;
             Console.WriteLine(packetLength);

            this.Invoke(new MethodInvoker(delegate ()
            {
                lstAny.Items.Add(PacketDetails(e)+"# Packet Size: "+packetLength+" bytes");
                lstAny.SelectedIndex = lstAny.Items.Count-1;
            }));
        }

        private void cmbCaptureMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            deviceMode = (SharpPcap.DeviceMode)Enum.Parse(typeof(SharpPcap.DeviceMode), cmbCaptureMode.SelectedItem.ToString());
        }

        private void lstAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAdapterName();
        }

        private void btnStopCapute_Click(object sender, EventArgs e)
        {
            // stop Capturing the packets
            try
            {
                device.StopCapture();
            }
            catch (Exception d)
            {
                Console.WriteLine(d.Message);
            }
        }

        private void btnSaveFilterSetting_Click(object sender, EventArgs e)
        {
            FilterIp = txtfilterIPaddress.Text;
        }

        private void cmbfilerProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filterprotocol = cmbfilerProtocol.SelectedItem.ToString();
        }

        public void AutoComplete()
        {
            // auto complete some of the text boxes in the system
                txtPacketSourceIP.Text = GetHalfIP();
                txtPacketDestinationIP.Text = GetHalfIP();
        }

        private void btnSendPacket_Click(object sender, EventArgs e)
        {
            // sends Packets
            string packetsendSourceIP = txtPacketSourceIP.Text;
            string packetsendDestinationIP = txtPacketDestinationIP.Text;
            string packetsendSourceMac = txtPacketSourceMac.Text;
            string packetsendDestination = txtPacketDestinationMac.Text;
            string packetsendProtocol = cmbPacketType.Text;
            int packetsendSourcePort = int.Parse(txtPacketSourcePort.Text);
            int packetsendDestinationPort = int.Parse(txtPaxketDestinationPort.Text);

            if (true)
            {
                packetToSend = Monitor.Packet(packetsendSourceIP, packetsendDestinationIP,
                packetsendSourcePort, packetsendDestinationPort, packetsendSourceMac,
                packetsendDestination, (EthernetPacketType)cmbPacketType.SelectedItem);
                device.SendPacket(packetToSend);
            }

        }

        public string GetHalfIP()
        {
            string node = Dns.GetHostName();
            string ip = Dns.GetHostByName(node).AddressList[0].ToString();
            string[] ipsplit = ip.Split('.');
            string halfIP = ipsplit[0] + "." + ipsplit[1] + "." + ipsplit[2] + ".";
            return halfIP;
        }





        /// <summary>
        /// WORK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>









        private void btnTryAutoComplete_Click(object sender, EventArgs e)
        {
            IPAddress IPsrc = null;
            IPAddress.TryParse(txtPacketSourceIP.Text, out IPsrc);
            IPAddress IPdes = null;
            IPAddress.TryParse(txtPacketDestinationIP.Text, out IPdes);

            if (MACnIPs.Count > 1)
            {
                foreach (var item in MACnIPs)
                {
                    if (item.Contains(IPdes.ToString())) {
                        txtPacketDestinationMac.Text = item.ToString();
                    }
                }
            }

            if(IPsrc!=null && IPdes != null)
            {
                NetworkInterface[] ni = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var item in ni)
                {
                    if (item.OperationalStatus == OperationalStatus.Up)
                    {
                        if (item.GetPhysicalAddress().ToString().Length > 5)
                        {
                            txtPacketSourceMac.Text = item.GetPhysicalAddress().ToString();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid IP Addresses");
            }
        }
    }
}
