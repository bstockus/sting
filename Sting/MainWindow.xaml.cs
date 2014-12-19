//Copyright 2014, Bryan Stocks
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

        private ObservableCollection<OldHost> hosts = new ObservableCollection<OldHost>();

        private int pingInterval = 1;
        private bool globalActive = true;

        public Boolean Active {
            get {
                return this.globalActive;
            }

            private set {
                this.globalActive = value;
                if (this.globalActive) {
                    string packUri = "pack://application:,,,/Sting;component/Images/playback_pause_icon.png";
                    btnPauseAll_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                } else {
                    string packUri = "pack://application:,,,/Sting;component/Images/playback_play_icon.png";
                    btnPauseAll_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }
                 
            }
        }

        private static int[] PING_INTERVALS = new int[] { 1, 2, 3, 5, 10, 15, 30, 60 };

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow() {
            InitializeComponent();
        }

        private void btnPauseAll_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnPauseAll_Click()");
            if (this.Active) {
                foreach (OldHost host in hosts) {
                    host.Active = false;
                }
                lstHosts.Items.Refresh();
                this.Active = false;
            } else {
                foreach (OldHost host in hosts) {
                    host.Active = true;
                }
                lstHosts.Items.Refresh();
                this.Active = true;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.bryanstockus.com/sting.html");
        }

        private void btnToolbarAdd_Click(object sender, RoutedEventArgs e) {

        }

        private void cboPingInterval_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cbo = (ComboBox)sender;
            pingInterval = PING_INTERVALS[cbo.SelectedIndex];
            foreach (OldHost host in hosts) {
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
            OldHost host = GetHostByGUID((String)(((Button)sender).Tag));
            host.Active = !host.Active;
            lstHosts.Items.Refresh();
        }

        private void btnRemoveHost_Click(object sender, RoutedEventArgs e) {
            OldHost host = GetHostByGUID((String)(((Button)sender).Tag));
            hosts.Remove(host);
            host.Terminate();
            lstHosts.Items.Refresh();
        }

        private OldHost GetHostByGUID(String GUID) {
            OldHost host = hosts.First(p => GUID.Equals(p.GUID));
            return host;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            foreach (OldHost host in hosts) {
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
            IPAddress _tempAddress;
            if (IPAddress.TryParse(dnsName, out _tempAddress)) {
                OldHost host = new OldHost(_tempAddress, this.pingInterval, this.Active);
                this.hosts.Add(host);
                lstHosts.Items.Refresh();
                txtNewAddress.Text = "";
                return;
            }
            try {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(dnsName);
                if (addresses != null) {
                    if (addresses.Length > 0) {
                        IPAddress address = addresses[0];
                        OldHost host = new OldHost(address, this.pingInterval, this.Active);
                        this.hosts.Add(host);
                        lstHosts.Items.Refresh();
                        txtNewAddress.Text = "";
                        host.DNSName = dnsName;
                        return;
                    }
                }
                throw new Exception();
            } catch (Exception e) {
                MessageBox.Show(this, "Unable to resolve host: " + dnsName + "!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
