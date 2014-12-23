using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {
    public class GroupedDnsHost : DnsBasicHost {

        public GroupedDnsHost(IPAddress ipAddress, String domainName, String groupName)
            : base(ipAddress, domainName) {
                this.groupName = groupName;
        }

    }
}
