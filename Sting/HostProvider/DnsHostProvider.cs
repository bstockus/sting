using Sting.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sting.HostProvider {
    public class DnsHostProvider : IHostProvider {
        public Host.BasicHost Host(string value) {
            return new DnsBasicHost(DnsService.Lookup(value), value);
        }

        public bool ValidHostValue(string value) {
            return true;
        }
    }
}
