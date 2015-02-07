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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sting.HostProvider {
    public class DnsHostProvider : IHostProvider {
        public Host.BasicHost Host(string value, IHostManager hostsManager) {
            return new DnsBasicHost(DnsService.Lookup(value), value);
        }

        public bool ValidHostValue(string value) {
            try {
                new Uri("http://" + value);
                return true;
            } catch (UriFormatException) {
                return false;
            }
        }
    }
}
