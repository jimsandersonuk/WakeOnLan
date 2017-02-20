using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using PingAllOnNetwork;
using WakeOnLan;
using ArpNetwork;
using FindMacAddress;


namespace WakeOnLan
{
    class Program
    {
        static void Main(string[] args)
        {
            var ip = new ActiveNetwork();
            //var activeIpList = new List<ActiveNetwork.ListIpAddress>(ip.ActiveIP());
            var arp = new ArpNetworkPcs();
            var activeIpList = new List<ArpNetworkPcs.LocalPcDetails>(arp.FindLocalPcs());
            var activeIpDetails = new List<ActiveNetwork.ListIpAddress>();
            foreach (var activeIP in activeIpList)
            {
                string checkPing = new ActiveNetwork().PingIpAddress(activeIP.IpAddress);
                var isActive = new ActiveNetwork.ListIpAddress {
                    IpAddress = checkPing
                };
                activeIpDetails.Add(isActive);
            }
            var mac = new FindNetworkPcs();
            var activeMacList = new List<FindNetworkPcs.MacAddressList>(mac.ActiveConnections());
            WakeUpOnLan.WakeUpPc();
            //var activeNewIpList = new List<ActiveNetwork.ListIpAddress>(ip.ActiveIP());
            //if (activeNewIpList.Contains("192.168.0.1") == true)       Console.Writeline("Hey")
        }
    }
}