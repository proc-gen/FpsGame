using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Server.Utils
{
    public static class IPAddressUtils
    {
        public static List<IPAddress> GetAllLocalIPv4()
        {  
            List<IPAddress> ips = new List<IPAddress>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {                
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();
                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {   
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {   
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                ips.Add(ip.Address);
                            }
                        }
                    }
                }
            }

            if(!ips.Any()) 
            {
                ips.Add(IPAddress.Loopback);
            }

            return ips;
        }
    }
}
