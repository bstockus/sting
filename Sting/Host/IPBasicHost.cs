using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {
    public class IPBasicHost : BasicHost {

        protected String title;
        protected String subTitle;

        public IPBasicHost(IPAddress ipAddress)
            : base(ipAddress) {
                this.title = ipAddress.ToString();
                this.subTitle = "";
        }

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

    }
}
