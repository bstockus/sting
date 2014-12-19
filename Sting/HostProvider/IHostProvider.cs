using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sting.HostProvider {
    public interface IHostProvider {

        IPAddress Lookup(String value);

        Boolean ValidHostValue(String value);

    }
}
