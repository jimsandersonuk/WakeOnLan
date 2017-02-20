// No copyrights(c). Use as you wish!

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;

namespace WakeOnLan
{
    // *************************************************************************
    /// <summary>
    /// A class to demonstrate Wake up on LAN. For this program to function, your
    /// machine must be setup to accept wake-up on LAN requests. Usually this
    /// option can set to "Enabled" state in boot(BIOS) options. You can see the
    /// set value by Rebooting your PC/Laptop and press F2 just after you power
    /// on your PC/Laptop. (you might have to keep pressing F2 until it comes up
    /// with boot options)
    /// </summary>
    /// <remarks>
    /// <para>
    /// For more information see http://support.microsoft.com/kb/257277 and
    /// http://en.wikipedia.org/wiki/Wake-on-LAN.
    /// </para>
    /// <para>For an in depth details please visit :
    /// http://en.wikipedia.org/wiki/Data_link_layer and
    /// http://en.wikipedia.org/wiki/Network_layer </para>
    /// </remarks>
    public sealed class WakeUpOnLan
    {
        // *********************************************************************
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="args">An array of command line arguments.</param>
        public static void WakeUpPc()
        {
            bool wakeUp = true;
            while (wakeUp)
            {
                Console.WriteLine("Enter the MAC address of the host to wake up " +
                    " on LAN (no space or hyphen(-). e.g. 0021702BA7A5." +
                    "Type Exit to end the program):");
                string macAddress = Console.ReadLine();

                StringComparer cp = StringComparer.OrdinalIgnoreCase;
                if (cp.Compare(macAddress, "Exit") == 0) break;

                //remove all non 0-9, A-F, a-f characters
                macAddress = Regex.Replace(macAddress, @"[^0-9A-Fa-f]", "");
                //check if mac adress length is valid
                if (macAddress.Length != 12)
                {
                    Console.WriteLine("Invalid MAC address. Try again!");
                }
                else
                {
                    Wakeup(macAddress);
                }
            }
        }

        // *********************************************************************
        /// <summary>
        /// Wakes up the machine with the given <paramref name="macAddress"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <note>
        /// <list type="number">
        /// <item> The motherboard must support Wake On LAN.</item>
        /// <item> The NIC must support Wake On LAN.</item>
        /// <item> There must be a wire connecting the motherboard's WOL port to
        /// the NIC's WOL port. Usually there always is a connection on most of
        /// the PCs.</item>
        /// <item> The Wake On LAN feature must be enabled in the motherboard's
        /// BIOS. Usually this is also enabled by default, but you might like to
        /// check again.</item>
        /// <item> The "Good Connection" light on the back of the NIC must be lit
        /// when the machine is off. (By default always good if you are not
        /// facing any network issues)</item>
        /// <item> Port 12287 (0x2FFF) must be open. (By default it should be
        /// open unless some antivirus or any other such program has changed
        /// settings.)</item>
        /// <item> Packets cannot be broadcast across the Internet.  That's why
        /// it's called Wake On Lan, not Wake On Internet.</item>
        /// <item> To find your MAC address, run the MSINFO32.EXE tool that is a
        /// part of Windows and navigate to Components > Network > Adapteror
        /// or simply type nbtstat -a &lt;your hostname &lt at command prompt.
        /// e.g. nbtstat -a mymachinename or nbtstat -A 10.2.100.213.
        /// .</item>
        /// </list>
        /// </note>
        /// </para>
        /// <para>You could also use my other blog on "Get MAC address" to simply
        /// integrate this program and my other program so you just have to input
        /// the hostname/IP address of the machine which you want woken up.</para>
        /// <para>See http://mycomponent.blogspot.com/2009/05/get-mac-address-in-c-from-ip.html
        /// </para>
        /// </remarks>
        /// <param name="macAddress">The MAC address of the host which has to be
        /// woken up.</param>
        private static void Wakeup(string macAddress)
        {
            WOLUdpClient client = new WOLUdpClient();
            client.Connect(
                    new IPAddress(0xffffffff),    //255.255.255.255  i.e broadcast
                    0x2fff); // port = 12287
                             // Set the socketoptions
            if (client.IsClientInBroadcastMode())
            {
                int byteCount = 0;
                // buffer to be sent:
                // 6 bytes each with 255 + 16 times mac each 6 bytes
                byte[] bytes = new byte[102];
                // The packet begins with 6 bytes trailer of FF bytes which is
                // followed by 16 times repeated MAC address of the target device
                // (i.e. the device that should be switched on). MAC Address is
                // used as an identifier in the packet, because that is the only
                // valuable identification that is available when the PC is not
                // running. MAC Address is assigned by the manufacturer (it is
                // a layer 2 - Data link layer identifier) and it stored in the
                // flash memory of the network card itself, so the network card
                // can perform the comparison very easily. It cannot use an IP
                // address, because network card simply does not have one when
                // PC is not running - IP address is a layer 3 - network layer
                // identifier, which means it is assigned by the OS.
                // You may also ask why the MAC address is repeated 16 times?
                // As mentioned above the network card scans all packets that
                // are coming in and it does not support any protocols of higher
                // levels (TCP, HTTP, etc.) - it will literally go through all
                // bytes in the packet and if it finds the "magic packet"
                // sequence anywhere in the data or even a packet header, it
                // will turn on the PC. Imagine that the packet did not repeat
                // the MAC Address, so it would only utilise 6 bytes of FF and
                // then 6 bytes of the MAC address. This 12 bytes combination
                // may sooner or later appear in your network communication
                // (in a file transfer, incoming email, a picture, etc.). 12
                // bytes is just not enough, which is why the MAC address is
                // repeated 16 times giving the packet solid 102 bytes. The
                // probability that those 102 bytes will unintentionally appear
                // in transferred data is exponentially lower (there are
                // 256^102 different packets which should be safe enough).

                // First 6 bytes should be 0xFF
                for (int trailer = 0; trailer < 6; trailer++)
                {
                    bytes[byteCount++] = 0xFF;
                }
                // Repeat MAC 16 times
                for (int macPackets = 0; macPackets < 16; macPackets++)
                {
                    int i = 0;
                    for (int macBytes = 0; macBytes < 6; macBytes++)
                    {
                        bytes[byteCount++] =
                            byte.Parse(macAddress.Substring(i, 2),
                            NumberStyles.HexNumber);
                        i += 2;
                    }
                }

                // Send wake up packet (the magic packet!)
                int returnValue = client.Send(bytes, byteCount);
                Console.WriteLine(returnValue + " bytes sent to " + macAddress +
                    Environment.NewLine + "Check if it's woken up. If not, try again!" +
                    Environment.NewLine);
            }
            else
            {
                Console.WriteLine("Remote client could not be set in broadcast mode!");
            }
        } // end Wakeup

    } // end class WakeUpOnLan

    // *************************************************************************
    /// <summary>
    /// A <see cref="UdpClient"/> class to set the client to broadcast mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class just sets the SocketOption to
    /// <see cref="SocketOptionName.Broadcast"/> mode.
    /// </para>
    /// </remarks>
    public class WOLUdpClient : UdpClient
    {
        // *********************************************************************
        /// <summary>
        /// Initializes a new instance of <see cref="WOLUdpClient"/>.
        /// </summary>
        public WOLUdpClient() : base()
        {
        }

        // *********************************************************************
        /// <summary>
        /// Sets up the UDP client to broadcast packets.
        /// </summary>
        /// <returns><see langword="true"/> if the UDP client is set in
        /// broadcast mode.</returns>
        public bool IsClientInBroadcastMode()
        {
            bool broadcast = false;
            if (this.Active)
            {
                try
                {
                    this.Client.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.Broadcast, 0);
                    broadcast = true;
                }
                catch
                {
                    broadcast = false;
                }
            }
            return broadcast;
        }

    } // end class WOLUdpClient
} // end namespace Yours.Truly

