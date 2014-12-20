// Copyright 2014, Bryan Stockus
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
            hostProviders.Add(new SpecialSubNetHostProvider());
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

        public void RemoveAllHost() {
            foreach (BasicHost host in this.hosts) {
                host.RemoveHost();
            }
            hosts.Clear();
        }

        public void ToggleHostPause(String guid) {
            System.Diagnostics.Debug.WriteLine("HostsManager.ToggleHostPause() guid='" + guid + "'");
            foreach (BasicHost host in this.hosts) {
                if (host.GUID.Equals(guid)) {
                    System.Diagnostics.Debug.WriteLine("HostsManager.ToggleHostPause() match='" + host.ToString() + "'");
                    if (host.IsPaused) {
                        host.UnPause();
                    } else {
                        host.Pause();
                    }
                    return;
                }
            }
        }

        public void PauseAllHosts() {
            foreach (BasicHost host in this.hosts) {
                host.Pause();
            }
        }

        public void UnPauseAllHosts() {
            foreach (BasicHost host in this.hosts) {
                host.UnPause();
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
