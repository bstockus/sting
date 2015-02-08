// Copyright 2014-2015, Bryan Stockus
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QuickSting {

    [Serializable]
    [XmlRoot("SpecialHosts")]
    public class SpecialHosts {

        [XmlAttribute("DnsPattern")]
        public string DnsPattern { get; set; }

        [XmlElement("Site")]
        public Site Site { get; set; }

        [XmlElement("Group")]
        public List<HostsGroup> Groups { get; set; }

    }

    [Serializable]
    public class HostsGroup {

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("Host")]
        public List<HostInformation> Hosts { get; set; }

    }

    public class HostProvider {

        private SpecialHosts specialHosts = null;

        private Dictionary<String, IPAddress> hostIPAddressCache = new Dictionary<string, IPAddress>();
        private Dictionary<String, HostDefinition> hostsCache = new Dictionary<string, HostDefinition>();

        public Site Site {
            get {
                return this.specialHosts.Site;
            }
        }

        public HostProvider() {
            this.specialHosts = ConfigFileHelper.LoadConfigFile<SpecialHosts>("SpecialHosts.xml");
            foreach (HostsGroup group in this.specialHosts.Groups) {
                foreach (HostInformation host in group.Hosts) {
                    this.hostsCache.Add(host.HostOctet, new HostDefinition {
                        HostOctet = host.HostOctet,
                        Name = host.Name,
                        GroupName = group.Name,
                        Description = host.Description,
                        Services = host.Services
                    });
                }
            }
        }

        private readonly object hostsCacheLock = new object();

        private IPAddress GetSiteAddress(String siteValue) {
            IPAddress siteAddress;
            lock (hostsCacheLock) {
                if (hostIPAddressCache.ContainsKey(siteValue)) {
                    siteAddress = hostIPAddressCache[siteValue];
                } else {
                    siteAddress = Lookup(specialHosts.DnsPattern.Replace("%%%SITE%%%", siteValue));
                    hostIPAddressCache.Add(siteValue, siteAddress);
                }
            }
            return siteAddress;
        }

        public HostCollection Host(string value) {
            HostCollection hostCollection = new HostCollection();

            IPAddress siteAddress = GetSiteAddress(value);

            foreach (HostDefinition host in hostsCache.Values) {
                Byte[] siteAddressBytes = siteAddress.GetAddressBytes();
                siteAddressBytes[3] = byte.Parse(host.HostOctet);
                IPAddress hostAddress = new IPAddress(siteAddressBytes);
                hostCollection.Add(new Host(host, hostAddress));
            }

            return hostCollection;
        }

        public static IPAddress Lookup(String value) {
            return Dns.GetHostEntry(value).AddressList[0];
        }

    }
}
