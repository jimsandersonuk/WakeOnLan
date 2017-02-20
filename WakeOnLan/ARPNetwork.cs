using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace ArpNetwork
{
     class ArpNetworkPcs
    {
       internal class LocalPcDetails
        {
            public string MacAddress { get; set; }
            public string IpAddress { get; set; }
            public string IpType { get; set; }
            public bool IpActive { get; set; }
        }

        /// <summary>
        /// Retrieves the IPInfo for All machines on the local network.
        /// </summary>
        /// <returns></returns>
        public List<LocalPcDetails> FindLocalPcs()
        {
            var activeLocalPcs = new List<LocalPcDetails>();
            activeLocalPcs =  LocalPing();
            return activeLocalPcs;
        }


        //private List<IpReturnDetails> LocalPing()
        //{
        //    // Ping's the local machine.
        //    Ping pingSender = new Ping();
        //    //IPAddress address = IPAddress.Loopback;
        //    var subNet = "192.168.0";
        //    var ipString = string.Empty;
        //    var activeIp = new List<IpReturnDetails>();
        //    for (var i = 0; i < 255; i++)
        //    {
        //        var address = IPAddress.Parse(string.Format("{0}.{1}", subNet, i));
        //        var reply = pingSender.Send(address);

        //        if (reply != null && reply.Status == IPStatus.Success)
        //        {

        //            var activeIpDetails = new IpReturnDetails()
        //            {
        //                IpAddress = reply.Address.ToString()

        //            };
        //            //Console.WriteLine("Address: {0}", reply.Address.ToString());
        //            //Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
        //            //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
        //            //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
        //            //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);

        //            activeIp.Add(activeIpDetails);
        //        }
        //        //else
        //        //{
        //        //    Console.WriteLine(reply.Status);
        //        //}
        //    }
        //}

        /// <summary>
        /// Retrieves the IPInfo for All machines on the local network.
        /// </summary>
        /// <returns></returns>
        private List<LocalPcDetails> LocalPing()//List<string> GetIPInfo()
        {
            var localPcs = new List<LocalPcDetails>();

            try
            {
                //var localPcs = new List<LocalPcDetails>();

                foreach (var arp in GetARPResult().Split(new char[] { '\n', '\r' }))
                {



                    // Parse out all the MAC / IP Address combinations
                    if (!string.IsNullOrEmpty(arp))
                    {
                        var pieces = (from piece in arp.Split(new char[] { ' ', '\t' })
                                      where !string.IsNullOrEmpty(piece)
                                      select piece).ToArray();
                        if (pieces.Length == 3)
                        {
                            var newIp = new LocalPcDetails
                            {
                                IpAddress = pieces[0],
                                MacAddress = pieces[1],
                                IpType = pieces[2]

                            };

                            localPcs.Add(newIp);
                        }
                    }
                }

                // Return list of IPInfo objects containing MAC / IP Address combinations
                
            }
            catch (Exception ex)
            {
                throw new Exception("IPInfo: Error Parsing 'arp -a' results", ex);
            }

            return localPcs;
        }

        /// <summary>
        /// This runs the "arp" utility in Windows to retrieve all the MAC / IP Address entries.
        /// </summary>
        /// <returns></returns>
        private static string GetARPResult()
        {
            Process p = null;
            string output = string.Empty;

            try
            {
                p = Process.Start(new ProcessStartInfo("arp", "-a")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });

                output = p.StandardOutput.ReadToEnd();

                p.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("IPInfo: Error Retrieving 'arp -a' Results", ex);
            }
            finally
            {
                if (p != null)
                {
                    p.Close();
                }
            }

            return output;
        }
    }
}
