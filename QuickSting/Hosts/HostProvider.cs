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
    public class HostProvider {

        [Serializable]
        [XmlRoot("SpecialHosts")]
        public class SpecialHostsConfig {

            [Serializable]
            public class SpecialHostConfig {

                [XmlAttribute("HostOctet")]
                public String HostOctet { get; set; }

                [XmlAttribute("Name")]
                public String DisplayName { get; set; }

                [XmlAttribute("Group")]
                public String GroupName { get; set; }

                [XmlAttribute("Description")]
                public String Description { get; set; }

                [XmlElement("Service")]
                public Service[] Services { get; set; }

            }

            public String DefaultDnsLookupSuffix { get; set; }

            [XmlElement("Host")]
            public List<SpecialHostConfig> SpecialHostConfigs { get; set; }

        }

        private SpecialHostsConfig specialHostsConfig = null;

        private Dictionary<String, IPAddress> hostsCache = new Dictionary<string, IPAddress>();
        private Dictionary<String, SpecialHostsConfig.SpecialHostConfig> specialHostConfigsCache = new Dictionary<string, SpecialHostsConfig.SpecialHostConfig>();

        public HostProvider() {
            this.specialHostsConfig = ConfigFileHelper.LoadConfigFile<SpecialHostsConfig>("SpecialHosts.xml");
            foreach (SpecialHostsConfig.SpecialHostConfig specialHostConfig in this.specialHostsConfig.SpecialHostConfigs) {
                this.specialHostConfigsCache.Add(specialHostConfig.HostOctet, specialHostConfig);
            }
        }

        private readonly object hostsCacheLock = new object();

        private IPAddress GetSiteAddress(String siteValue) {
            IPAddress siteAddress;
            lock (hostsCacheLock) {
                if (hostsCache.ContainsKey(siteValue)) {
                    siteAddress = hostsCache[siteValue];
                } else {
                    siteAddress = Lookup(siteValue + specialHostsConfig.DefaultDnsLookupSuffix);
                    hostsCache.Add(siteValue, siteAddress);
                }
            }
            return siteAddress;
        }

        public HostCollection Host(string value) {

            HostCollection hostCollection = new HostCollection();

            IPAddress siteAddress = GetSiteAddress(value);

            foreach (SpecialHostsConfig.SpecialHostConfig host in specialHostConfigsCache.Values) {
                Byte[] siteAddressBytes = siteAddress.GetAddressBytes();
                siteAddressBytes[3] = byte.Parse(host.HostOctet);
                IPAddress hostAddress = new IPAddress(siteAddressBytes);
                hostCollection.Add(new Host(new HostInformation {
                    Name = host.DisplayName,
                    GroupName = host.GroupName,
                    Services = host.Services,
                    Description = host.Description
                }, hostAddress));
            }

            return hostCollection;

        }

        public static IPAddress Lookup(String value) {
            return Dns.GetHostEntry(value).AddressList[0];
        }

    }
}
