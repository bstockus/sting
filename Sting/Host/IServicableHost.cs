using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {
    public interface IServicableHost {

        void PerformVncService();

        void PerformShellService();

        void PerformWebService();

    }
}
