using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sting {
    public class DnsService {

        public static IPAddress Lookup(String value) {
            return Dns.GetHostEntry(value).AddressList[0];
        }

    }
}
