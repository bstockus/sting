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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sting {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private HostsManager hostsManager = new HostsManager();

        private int pingInterval = 1;

        public HostsManager HostsManager {
            get {
                return this.hostsManager;
            }
        }

        public Boolean IsPaused { get; set; }

        private static int[] PING_INTERVALS = new int[] { 1, 2, 3, 5, 10, 15, 30, 60 };

        private System.Windows.Threading.DispatcherTimer pingDispatchTimer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer updateDispatchTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow() {
            this.IsPaused = false;
            InitializeComponent();
        }

        private void btnPauseAll_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnPauseAll_Click()");
            if (!this.IsPaused) {
                string packUri = "pack://application:,,,/Sting;component/Images/playback_play_icon.png";
                btnPauseAll_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                this.IsPaused = true;
                this.pingDispatchTimer.Stop();
                this.updateDispatchTimer.Stop();
                lstHosts.Effect = new BlurEffect();
            } else {
                string packUri = "pack://application:,,,/Sting;component/Images/playback_pause_icon.png";
                btnPauseAll_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                this.IsPaused = false;
                this.pingDispatchTimer.Start();
                this.updateDispatchTimer.Start();
                lstHosts.IsEnabled = true;
                lstHosts.Effect = null;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.bryanstockus.com/sting.html");
        }

        private void cboPingInterval_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cbo = (ComboBox)sender;
            pingInterval = PING_INTERVALS[cbo.SelectedIndex];
            this.pingDispatchTimer.Interval = new TimeSpan(0, 0, this.pingInterval);
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.lstHosts.ItemsSource = this.hostsManager.Hosts;

            this.pingDispatchTimer.Tick += new EventHandler(pingDispatchTimer_Tick);
            this.pingDispatchTimer.Interval = new TimeSpan(0, 0, this.pingInterval);
            this.pingDispatchTimer.Start();
            this.updateDispatchTimer.Tick += new EventHandler(updateDispatchTimer_Tick);
            this.updateDispatchTimer.Interval = new TimeSpan(0, 0, 1);
            this.updateDispatchTimer.Start();
        }

        private void btnPauseHost_Click(object sender, RoutedEventArgs e) {
            if (!this.IsPaused) {

            }
        }

        private void btnRemoveHost_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnRemoveHost_Click()");
            this.HostsManager.RemoveHost((String)((Button)sender).Tag);
            lstHosts.Items.Refresh();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            
        }

        private void pingDispatchTimer_Tick(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.pingDispatchTimer_Tick()");
            this.HostsManager.PingHosts();
        }

        private void updateDispatchTimer_Tick(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.updateDispatchTimer_Tick()");
            lstHosts.Items.Refresh();
        }

        private void txtNewAddress_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                this.AddNewHost(txtNewAddress.Text);
            }
        }

        void AddNewHost(String value) {
            this.hostsManager.AddHost(value, this);  
        }


    }
}
