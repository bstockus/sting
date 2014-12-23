using Sting.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sting.HostProvider {
    public class SpecialSubNetHostProvider : IHostProvider {

        private Dictionary<String, IPAddress> hostsCache = new Dictionary<string, IPAddress>();

        public BasicHost Host(string value) {
            string siteValue = value.Substring(1, 3);
            string hostValue = value.Substring(4);

            int _tempIntValue;
            IPAddress hostAddress;
            if (!int.TryParse(hostValue, out _tempIntValue)) {
                hostAddress = DnsService.Lookup(value.Substring(1));
            } else {
                IPAddress siteAddress;
                if (hostsCache.ContainsKey(siteValue)) {
                    siteAddress = hostsCache[siteValue];
                } else {
                    siteAddress = DnsService.Lookup(siteValue + "str");
                    hostsCache.Add(siteValue, siteAddress);
                }

                byte[] siteAddressBytes = siteAddress.GetAddressBytes();
                siteAddressBytes[3] = byte.Parse(hostValue);
                hostAddress = new IPAddress(siteAddressBytes);
            }
            
            return new GroupedDnsHost(hostAddress, siteValue + "/" + hostValue, siteValue);
        }

        public bool ValidHostValue(string value) {
            return value.StartsWith("/");
        }
    }
}
