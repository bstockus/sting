using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Sting.HostProvider {
    public class IPHostProvider : IHostProvider {

        public IPAddress Lookup(string value) {
            return IPAddress.Parse(value);
        }

        public bool ValidHostValue(string value) {
            IPAddress _addr;
            return IPAddress.TryParse(value, out _addr);
        }
    }
}
