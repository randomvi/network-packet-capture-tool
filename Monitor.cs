using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using SharpPcap.WinPcap;
using SharpPcap.LibPcap;
using SharpPcap.AirPcap;
using PacketDotNet.Tcp;
using PacketDotNet;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
namespace NETAPP
{
    class Monitor
    {
        // monitors system
        public static List<string> GetListOfNetAdaptDevices()
        {
            // get adapters
            List<string> devs = new List<string>();
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                System19.Say("I Could not find any Network Devices within the system.");
            }
            foreach (ICaptureDevice device in devices)
            {
               devs.Add(device.Description);
            }
            // Might Neeed to go into details with the Device list of Available

             return devs;
        }

        public void CapturePacket2(ICaptureDevice device, DeviceMode captureMode, int timeOutMillis)
        {
            // without events
            device.Open(captureMode, timeOutMillis);
       //     System19.Say("Im Gonna use device "+device.Name+" to Extract Packets ");
            Packet packet = null;
            device.Close();
        }

        public void WritePacketsToFile(ICaptureDevice device, string filename)
        {
            //    CaptureFileWriterDevice packetWriter = new CaptureFileWriterDevice(device, filename);

        }

        public void ExtractingPacketInformation()
        {
            // used within the event
           
            /*
            var tcp = (TcpPacket)e.Packet.Extract(typeof(TcpPacket));
            //Console.WriteLine(tcp.SourceAddress);
           // Console.WriteLine(tcp.WindowSize);
            */
        }

        public void SendingPACKETS(ICaptureDevice device)
        {
            device.Open();
            byte[] bytes = new byte[1];// GetRandomPacket();
            try
            {
                device.SendPacket(bytes);
                Console.WriteLine("Packet sent");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            device.Close();
        }
        public void ConstructPacket(ICaptureDevice device)
        {
            // open device
            SendQueue QPackets = new SendQueue((int)((CaptureFileReaderDevice)device).FileSize);
            Packet packet;
        }

        public static EthernetPacket Packet(string ipsrc,string ipdes,int portsrc,int portdes,string macsrc,string macdes,EthernetPacketType packetType)
        {
            TcpPacket tcpPacket = new TcpPacket(ushort.Parse(portsrc.ToString()),ushort.Parse(portdes.ToString())); // ports # TCP Packet

            IPAddress IpSourceAddress = IPAddress.Parse(ipsrc);
            IPAddress IpDestinationAddress = IPAddress.Parse(ipdes);

            IPv4Packet ipPacket =new IPv4Packet(IpSourceAddress, IpDestinationAddress);// IP addresses # IP Packets

            string SourceMacAddress = macsrc;
            string DestinationMacAddress =macdes;
            //convert above value to a Physical Address

            PhysicalAddress MACsource = PhysicalAddress.Parse(SourceMacAddress);
            PhysicalAddress MACdestination = PhysicalAddress.Parse(DestinationMacAddress);

            EthernetPacket eth0Packet = new EthernetPacket(MACsource, MACdestination,packetType); // MAC address # Eth0 Packet

            // this below does Packet construction Encapsulation
            ipPacket.PayloadPacket = tcpPacket;
            eth0Packet.PayloadPacket = ipPacket;

            Console.WriteLine(eth0Packet.ToString());
            // allows us to retrieve bytes from the packets
            byte[] packetBytes = eth0Packet.Bytes;

            return eth0Packet; 
        }
    }
}
