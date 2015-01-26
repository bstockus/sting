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

using Sting.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sting.HostProvider {
    public class HostGroupProvider : IHostProvider {

        [Serializable]
        [XmlRoot("HostGroups")]
        public class HostGroupsConfig {

            [Serializable]
            public class HostGroupConfig {

                [XmlAttribute("Name")]
                public String Name { get; set; }

                [XmlElement("Host")]
                public List<String> Hosts { get; set; }

            }

            [XmlElement("Group")]
            public List<HostGroupConfig> HostGroups { get; set; }

        }

        private HostGroupsConfig hostGroupsConfig = null;

        private Dictionary<String, HostGroupsConfig.HostGroupConfig> hostGroupsCache = new Dictionary<string, HostGroupsConfig.HostGroupConfig>();

        public HostGroupProvider() {
            this.hostGroupsConfig = ConfigFileHelper.LoadConfigFile<HostGroupsConfig>("HostGroups.xml");
            foreach (HostGroupsConfig.HostGroupConfig hostGroupConfig in this.hostGroupsConfig.HostGroups) {
                this.hostGroupsCache.Add(hostGroupConfig.Name, hostGroupConfig);
            }
        }

        public BasicHost Host(string value, HostsManager hostsManager) {
            string[] splits = value.Split('#');
            string siteValue = splits[0];
            string groupValue = splits[1];

            foreach (String host in this.hostGroupsCache[groupValue].Hosts) {
                Task.Factory.StartNew(() => {
                    hostsManager.AddHost(siteValue + "/" + host);
                });
            }

            return null;
        }

        public bool ValidHostValue(string value) {
            return value.Contains("#");
        }

    }
}
