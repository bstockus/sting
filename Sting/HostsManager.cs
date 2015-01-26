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
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Sting {

    public class UnableToCreateHostException : Exception {

    }

    public class HostsManager : INotifyPropertyChanged {

        private ObservableCollection<BasicHost> hosts = new ObservableCollection<BasicHost>();

        private List<IHostProvider> hostProviders = new List<IHostProvider>();

        private int pingInterval = 1;

        private Boolean paused = false;

        public Boolean IsPaused {
            get {
                return this.paused;
            }
            set {
                this.paused = value;
                this.OnPropertyChanged("IsPaused");

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private static int[] PING_INTERVALS = new int[] { 1, 2, 3, 5, 10, 15, 30, 60 };

        private System.Windows.Threading.DispatcherTimer pingDispatchTimer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer updateDispatchTimer = new System.Windows.Threading.DispatcherTimer();

        public ObservableCollection<BasicHost> Hosts {
            get {
                return this.hosts;
            }
        }

        public HostsManager() {
            this.IsPaused = false;

            this.pingDispatchTimer.Tick += new EventHandler(pingDispatchTimer_Tick);
            this.pingDispatchTimer.Interval = new TimeSpan(0, 0, this.pingInterval);
            this.updateDispatchTimer.Tick += new EventHandler(updateDispatchTimer_Tick);
            this.updateDispatchTimer.Interval = new TimeSpan(0, 0, 1);

            hostProviders.Add(new IPHostProvider());
            hostProviders.Add(new SpecialSubNetHostProvider());
            hostProviders.Add(new DnsHostProvider());
        }

        public void AddHost(String value, MainWindow mainWindow) {
            Task.Factory.StartNew(() => {
                foreach (IHostProvider hostProvider in this.hostProviders) {
                    if (hostProvider.ValidHostValue(value)) {
                        try {
                            BasicHost host = hostProvider.Host(value);
                            mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {
                                if (hosts.Count == 0) {
                                    this.updateDispatchTimer.Start();
                                    this.pingDispatchTimer.Start();
                                }
                                hosts.Add(host);
                                host.Ping().Start();
                            }));
                            return;
                        } catch (BasicHostAllReadyExistsException e) {
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

        public void RemoveHost(BasicHost host) {
            host.RemoveHost();
            hosts.Remove(host);
            if (hosts.Count == 0) {
                this.updateDispatchTimer.Stop();
                this.pingDispatchTimer.Stop();
            }
        }

        public void RemoveAllHost() {
            this.updateDispatchTimer.Stop();
            this.pingDispatchTimer.Stop();
            foreach (BasicHost host in this.hosts) {
                host.RemoveHost();
            }
            hosts.Clear();
        }

        public void SetPingInterval(int index) {
            pingInterval = PING_INTERVALS[index];
            this.pingDispatchTimer.Interval = new TimeSpan(0, 0, this.pingInterval);
        }

        public void ToggleHostPause(BasicHost host) {
            if (host.IsPaused) {
                host.UnPause();
            } else {
                host.Pause();
            }
        }

        public void ToggleAllHostsPause() {
            System.Diagnostics.Debug.WriteLine("HostsManager.btnPauseAll_Click()");
            if (!this.IsPaused) {
                this.IsPaused = true;
                this.pingDispatchTimer.Stop();
                this.updateDispatchTimer.Stop();
            } else {
                this.IsPaused = false;
                if (this.Hosts.Count > 0) {
                    this.pingDispatchTimer.Start();
                    this.updateDispatchTimer.Start();
                }
            }
        }

        public void PingHosts() {
            Task.Factory.StartNew(() => {
                foreach (BasicHost host in this.hosts) {
                    host.Ping().Start();
                }
            });
        }

        private void pingDispatchTimer_Tick(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("HostManager.pingDispatchTimer_Tick()");
            if (!this.IsPaused) {
                this.PingHosts();
            }
        }

        private void updateDispatchTimer_Tick(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("HostManager.updateDispatchTimer_Tick()");
            foreach (BasicHost host in this.Hosts) {
                host.OnPropertyChanged(new PropertyChangedEventArgs("Status"));
            }
        }

    }
}
