using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {
    public class DnsBasicHost : BasicHost {

        protected String title;
        protected String subTitle;

        public override string Title {
            get {
                return this.title;
            }
        }

        public override string SubTitle {
            get {
                return this.subTitle;
            }
        }

        public DnsBasicHost(IPAddress ipAddress, String domainName)
            : base(ipAddress) {
                this.title = domainName;
                this.subTitle = ipAddress.ToString();
        }
    }
}
