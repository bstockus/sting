using Sting.Host;
using Sting.HostProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Sting {

    public class UnableToCreateHostException : Exception {

    }

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
            hostProviders.Add(new DnsHostProvider());
        }

        public void AddHost(String value, MainWindow mainWindow) {
            Task.Run(() => {
                foreach (IHostProvider hostProvider in this.hostProviders) {
                    if (hostProvider.ValidHostValue(value)) {
                        try {
                            BasicHost host = hostProvider.Host(value);
                            mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {
                                hosts.Add(host);
                            }));
                            return;
                        } catch (Exception e) {
                            System.Diagnostics.Debug.WriteLine(e.ToString());
                        }
                    }
                }
                mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {
                    MessageBox.Show(mainWindow, "Unable to find '" + value + "'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            });
        }

        public void RemoveHost(String guid) {
            System.Diagnostics.Debug.WriteLine("HostsManager.RemoveHost() guid='" + guid + "'");
            foreach (BasicHost host in this.hosts) {
                if (host.GUID.Equals(guid)) {
                    System.Diagnostics.Debug.WriteLine("HostsManager.RemoveHost() match='" + host.ToString() + "'");
                    host.RemoveHost();
                    hosts.Remove(host);
                    return;
                }
            }
        }

        public void PingHosts() {

            foreach (BasicHost host in this.hosts) {
                if (!host.IsPaused) {
                    System.Diagnostics.Debug.WriteLine("Sent Ping to " + host.IPAddress.ToString());
                    host.Ping().Start();
                }
            }
        }

    }
}
