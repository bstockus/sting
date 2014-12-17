// Copyright 2014, Bryan Stocks
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sting {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private ObservableCollection<Host> hosts = new ObservableCollection<Host>();

        private int pingInterval = 1;
        private bool globalActive = true;

        private static int[] PING_INTERVALS = new int[] { 1, 2, 3, 5, 10, 15, 30, 60 };

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow() {
            InitializeComponent();
        }

        private void btnPauseAll_Click(object sender, RoutedEventArgs e) {
            if (globalActive) {
                foreach (Host host in hosts) {
                    host.Active = false;
                }
                btnPauseAll.Content = "Resume All";
                lstHosts.Items.Refresh();
                globalActive = false;
            } else {
                foreach (Host host in hosts) {
                    host.Active = true;
                }
                btnPauseAll.Content = "Pause All";
                lstHosts.Items.Refresh();
                globalActive = true;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.google.com");
        }

        private void btnToolbarAdd_Click(object sender, RoutedEventArgs e) {

        }

        private void cboPingInterval_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cbo = (ComboBox)sender;
            pingInterval = PING_INTERVALS[cbo.SelectedIndex];
            foreach (Host host in hosts) {
                host.PingInterval = pingInterval;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.lstHosts.ItemsSource = this.hosts;

            this.dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            this.dispatcherTimer.Start();
        }

        private void btnPauseHost_Click(object sender, RoutedEventArgs e) {
            Host host = GetHostByGUID((String)(((Button)sender).Tag));
            host.Active = !host.Active;
            lstHosts.Items.Refresh();
        }

        private void btnRemoveHost_Click(object sender, RoutedEventArgs e) {
            Host host = GetHostByGUID((String)(((Button)sender).Tag));
            hosts.Remove(host);
            host.Terminate();
            lstHosts.Items.Refresh();
        }

        private Host GetHostByGUID(String GUID) {
            Host host = hosts.First(p => GUID.Equals(p.GUID));
            return host;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            foreach (Host host in hosts) {
                host.Terminate();
            }
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            lstHosts.Items.Refresh();
        }

        private void txtNewAddress_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                this.AddNewHost(txtNewAddress.Text);
            }
        }

        async void AddNewHost(String dnsName) {
            try {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(dnsName);
                if (addresses != null) {
                    if (addresses.Length > 0) {
                        IPAddress address = addresses[0];
                        Host host = new Host(address, this.pingInterval, this.globalActive);
                        IPAddress _tempAddress;
                        if (!IPAddress.TryParse(dnsName, out _tempAddress)) {
                            host.DNSName = dnsName;
                        }
                        this.hosts.Add(host);
                        lstHosts.Items.Refresh();
                        txtNewAddress.Text = "";
                        return;
                    }
                }
            } catch (Exception e) {

            }
            MessageBox.Show(this, "Unable to resolve host: " + dnsName + "!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }


    }
}
