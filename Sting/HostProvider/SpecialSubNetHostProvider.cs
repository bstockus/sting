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

            IPAddress siteAddress;

            if (hostsCache.ContainsKey(siteValue)) {
                siteAddress = hostsCache[siteValue];
            } else {
                siteAddress = DnsService.Lookup(siteValue + "str");
                hostsCache.Add(siteValue, siteAddress);
            }

            byte[] siteAddressBytes = siteAddress.GetAddressBytes();
            siteAddressBytes[3] = byte.Parse(hostValue);
            return new DnsBasicHost(new IPAddress(siteAddressBytes), siteValue + "/" + hostValue);
        }

        public bool ValidHostValue(string value) {
            return value.StartsWith("/");
        }
    }
}
