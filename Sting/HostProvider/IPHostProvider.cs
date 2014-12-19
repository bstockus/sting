using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Sting.Host;

namespace Sting.HostProvider {
    public class IPHostProvider : IHostProvider {

        public BasicHost Host(string value) {
            System.Diagnostics.Debug.WriteLine("IPHostProvider.Host()");
            return new IPBasicHost(IPAddress.Parse(value));
        }

        public bool ValidHostValue(string value) {
            System.Diagnostics.Debug.WriteLine("IPHostProvider.ValidHostValue()");
            IPAddress _addr;
            return IPAddress.TryParse(value, out _addr);
        }
    }
}
