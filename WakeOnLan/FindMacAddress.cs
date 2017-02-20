using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FindMacAddress
{
    class FindNetworkPcs
    {

        internal class MacAddressList
        {
            internal string NicName { get; set; }
            internal string MacId { get; set; }
            internal string IpAddress { get; set; }
            internal string InterfaceType { get; set; }
            internal long? NicSpeed { get; set; }

        }
        /// <summary>
        /// Retrieves the IPInfo for All machines on the local network.
        /// </summary>
        /// <returns></returns>
        public List<MacAddressList> ActiveConnections()
        {
            var lanMacAddress = new List<MacAddressList>();
            lanMacAddress = FindAllMacAddressOnLan();
            return lanMacAddress;
        }

        internal List<MacAddressList> FindAllMacAddressOnLan()
        {
            const int minMacAddrLength = 12;
            var macAddress = string.Empty;
            var maxSpeed = -1;
            var listAllMac = new List<MacAddressList>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {

                var ipProps = nic.GetIPProperties();
                var ipAddress = string.Empty;
                string tempMac = nic.GetPhysicalAddress().ToString();

                if (nic.Speed > maxSpeed && !string.IsNullOrEmpty(tempMac) && tempMac.Length >= minMacAddrLength)
                {

                    foreach (var ip in ipProps.UnicastAddresses)
                    {
                        if ((nic.OperationalStatus == OperationalStatus.Up) && (ip.Address.AddressFamily == AddressFamily.InterNetwork))
                        {
                            ipAddress = ip.Address.ToString();
                        }
                    }

                    var validMac = new MacAddressList
                    {
                        NicName = nic.Description,
                        IpAddress = ipAddress,
                        MacId = nic.GetPhysicalAddress().ToString(),
                        NicSpeed = nic.Speed,
                        InterfaceType = nic.NetworkInterfaceType.ToString()
                    };

                    if ((nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet) && validMac.IpAddress.Length >1)
                        listAllMac.Add(validMac);
                }

            }
            return listAllMac;
        }


    }
}
