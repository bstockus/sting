using Sting.Host;
using Sting.HostProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sting {
    public class HostsManager {

        ObservableCollection<BasicHost> hosts = new ObservableCollection<BasicHost>();

        List<IHostProvider> hostProviders = new List<IHostProvider>();

        public ObservableCollection<BasicHost> Hosts {
            get {
                return this.hosts;
            }
        }

        public HostsManager() {
            hostProviders.Add(new IPHostProvider());
        }

    }
}
