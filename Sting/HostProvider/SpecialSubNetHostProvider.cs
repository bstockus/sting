﻿using Sting.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sting.HostProvider {
    public class SpecialSubNetHostProvider : IHostProvider {

        [Serializable]
        [XmlRoot("SpecialHosts")]
        public class SpecialHostsConfig {

            [Serializable]
            public class SpecialHostConfig {

                [XmlAttribute("HostOctet")]
                public String HostOctet { get; set; }

                [XmlAttribute("Name")]
                public String DisplayName { get; set; }

                [XmlElement("VncParams")]
                public VncHostParams VncHostParams { get; set; }

                [XmlElement("WebParams")]
                public List<WebHostParams> WebHostParams { get; set; }

                [XmlElement("Alias")]
                public List<String> Aliases { get; set; }

            }

            public String DefaultDnsLookupSuffix { get; set; }

            [XmlElement("Host")]
            public List<SpecialHostConfig> SpecialHostConfigs { get; set; }

        }

        private SpecialHostsConfig specialHostsConfig = null;

        private Dictionary<String, IPAddress> hostsCache = new Dictionary<string, IPAddress>();
        private Dictionary<String, SpecialHostsConfig.SpecialHostConfig> specialHostConfigsCache = new Dictionary<string, SpecialHostsConfig.SpecialHostConfig>();

        private Dictionary<String, String> hostAliasesCache = new Dictionary<string, string>();

        public SpecialSubNetHostProvider() {
            this.specialHostsConfig = ConfigFileHelper.LoadConfigFile<SpecialHostsConfig>("SpecialHosts.xml");
            foreach (SpecialHostsConfig.SpecialHostConfig specialHostConfig in this.specialHostsConfig.SpecialHostConfigs) {
                this.specialHostConfigsCache.Add(specialHostConfig.HostOctet, specialHostConfig);
                foreach (string hostAlias in specialHostConfig.Aliases) {
                    this.hostAliasesCache.Add(hostAlias.ToLower(), specialHostConfig.HostOctet);
                }
            }
        }

        public BasicHost Host(string value) {
            string[] splits = value.Split('/');
            string siteValue = splits[0];
            string hostValue = splits[1];

            if (this.hostAliasesCache.ContainsKey(hostValue.ToLower())) {
                hostValue = this.hostAliasesCache[hostValue.ToLower()];
            }

            int _tempIntValue;
            IPAddress hostAddress;
            if (!int.TryParse(hostValue, out _tempIntValue)) {
                hostAddress = DnsService.Lookup(siteValue + hostValue);
            } else {
                IPAddress siteAddress;
                if (hostsCache.ContainsKey(siteValue)) {
                    siteAddress = hostsCache[siteValue];
                } else {
                    siteAddress = DnsService.Lookup(siteValue + specialHostsConfig.DefaultDnsLookupSuffix);
                    hostsCache.Add(siteValue, siteAddress);
                }

                byte[] siteAddressBytes = siteAddress.GetAddressBytes();
                siteAddressBytes[3] = byte.Parse(hostValue);
                hostAddress = new IPAddress(siteAddressBytes);
            }

            String lastOctet = hostAddress.GetAddressBytes()[3].ToString();
            if (specialHostConfigsCache.ContainsKey(lastOctet)) {
                return new ServiceGroupedDnsHost(hostAddress, specialHostConfigsCache[lastOctet].DisplayName, siteValue, specialHostConfigsCache[lastOctet].VncHostParams, specialHostConfigsCache[lastOctet].WebHostParams.ToArray());
            } else {
                return new GroupedDnsHost(hostAddress, "." + lastOctet, siteValue);
            }
            
        }

        public bool ValidHostValue(string value) {
            return value.Contains("/");
        }
    }
}