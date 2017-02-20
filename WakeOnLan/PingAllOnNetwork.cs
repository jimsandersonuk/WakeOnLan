
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System;


namespace PingAllOnNetwork
{
    public sealed class ActiveNetwork
    {
        private static List<string> activeIp = new List<string>();
        private static List<Ping> pingers = new List<Ping>();
        private static int instances = 0;
        private static object @lock = new object();
        private static int result = 0;
        private static int timeOut = 250;
        private static int ttl = 5;

        public class ListIpAddress
        {
            public string IpAddress { get; set; }
        }
        public List<ListIpAddress> ActiveIP()
        {
            activeIp.Clear();
            var ipAddressList = new List<ListIpAddress>();
            _PingAll("192.168.0.", 255);
            foreach (var ip in activeIp)
            {
                var newIp = new ListIpAddress { IpAddress = ip.ToString() };

                ipAddressList.Add(newIp);
            }
            return ipAddressList;
        }
        public string PingIpAddress(string PingIpAddress)
        {
            activeIp.Clear();
            //var ipAddressList = new List<ListIpAddress>();
            _PingAll(PingIpAddress,1);
            var newIp = string.Empty;
            if (activeIp.Count > 0)
            {
                foreach (var ip in activeIp)
                {
                    newIp = ip.ToString();

                    //ipAddressList.Add(newIp);
                }
                 return newIp;
            }
            else return null;
        }

        private void _PingAll( string baseIp, int createPingers)
        {

            //Console.WriteLine("Pinging 255 destinations of D-class in {0}*", baseIP);

            _CreatePingers(createPingers);

            PingOptions po = new PingOptions(ttl, true);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] data = enc.GetBytes("abababababababababababababababab");

            SpinWait wait = new SpinWait();
            int cnt = 1;

            Stopwatch watch = Stopwatch.StartNew();

            foreach (Ping p in pingers)
            {
                lock (@lock)
                {
                    instances += 1;
                }

                if (baseIp.EndsWith("."))
                p.SendAsync(string.Concat(baseIp, cnt.ToString()), timeOut, data, po);
                else
                    p.SendPingAsync(baseIp, timeOut, data, po);
                cnt += 1;
            }

            while (instances > 0)
            {
                wait.SpinOnce();
            }

            watch.Stop();

            _DestroyPingers();

            //Console.WriteLine("Finished in {0}. Found {1} active IP-addresses.", watch.Elapsed.ToString(), result);
            //Console.ReadKey();

        }

        private void _Pingcompleted(object s, PingCompletedEventArgs e)
        {
            lock (@lock)
            {
                instances -= 1;
            }

            if (e.Reply.Status == IPStatus.Success)
            {
                activeIp.Add(e.Reply.Address.ToString());
               // Console.WriteLine(string.Concat("Active IP: ", e.Reply.Address.ToString()));
                result += 1;
            }
            else
            {
               // Console.WriteLine(String.Concat("Non-active IP: ", e.Reply.Address.ToString()));
            }
        }

        private void _CreatePingers(int cnt)
        {
            for (int i = 1; i <= cnt; i++)
            {
                Ping p = new Ping();
                p.PingCompleted += _Pingcompleted;
                pingers.Add(p);
            }
        }

        private void _DestroyPingers()
        {
            foreach (Ping p in pingers)
            {
                p.PingCompleted -= _Pingcompleted;
                p.Dispose();
            }

            pingers.Clear();

        }

    }
}
