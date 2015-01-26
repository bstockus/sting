// Copyright 2014-2015, Bryan Stockus
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
using System.ComponentModel;
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
    
    public partial class MainWindow : Window, INotifyPropertyChanged {

        private HostsManager hostsManager;

        public HostsManager HostsManager {
            get {
                return this.hostsManager;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public MainWindow() {
            this.hostsManager = new HostsManager(this);
            InitializeComponent();
        }

        private void btnPauseAll_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnPauseAll_Click()");
            this.HostsManager.ToggleAllHostsPause();
            if (this.HostsManager.IsPaused) {
                string packUri = "pack://application:,,,/Sting;component/Images/playback_play_icon.png";
                btnPauseAll_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                lstHosts.Effect = new BlurEffect();
                lstHosts.IsEnabled = false;
            } else {
                string packUri = "pack://application:,,,/Sting;component/Images/playback_pause_icon.png";
                btnPauseAll_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                lstHosts.Effect = null;
                lstHosts.IsEnabled = true;
            }
        }

        private void btnNotifyToggle_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnNotifyToggle_Click()");
            if (!NotificationManager.GetNotificationManager().IsEnabled) {
                string packUri = "pack://application:,,,/Sting;component/Images/eye_inv_icon.png";
                btnNotifyToggle_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                NotificationManager.GetNotificationManager().IsEnabled = true;
            } else {
                string packUri = "pack://application:,,,/Sting;component/Images/invisible_revert_icon.png";
                btnNotifyToggle_img.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                NotificationManager.GetNotificationManager().IsEnabled = false;
            }
        }
        private void btnRemoveAllHosts_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnRemoveAllHosts_Click()");
            this.HostsManager.RemoveAllHost();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnSettings_Click()");
            
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnAbout_Click()");
            System.Diagnostics.Process.Start("http://www.bryanstockus.com/sting.html");
        }

        private void cboPingInterval_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.cboPingInterval_SelectionChanged()");
            ComboBox cbo = (ComboBox)sender;
            this.HostsManager.SetPingInterval(cbo.SelectedIndex);
        }
        private void tbMain_Loaded(object sender, RoutedEventArgs e) {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null) {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null) {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.lstHosts.ItemsSource = this.hostsManager.Hosts;

            ICollectionView view = CollectionViewSource.GetDefaultView(lstHosts.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            view.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            
        }

        private void txtNewAddress_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                this.AddNewHost(txtNewAddress.Text);
            }
        }

        void AddNewHost(String value) {
            this.hostsManager.AddHostWithErrorPopup(value);  
        }

        private void btnPauseHost_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnPauseHost_Click()");
            BasicHost host = (BasicHost)(((Button)sender).DataContext);
            this.HostsManager.ToggleHostPause(host);
        }

        private void btnRemoveHost_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnRemoveHost_Click()");
            BasicHost host = (BasicHost)(((Button)sender).DataContext);
            this.HostsManager.RemoveHost(host);
        }

        private void btnOpenVnc_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnOpenVnc_Click()");
            IServicableHost host = (IServicableHost)(((Button)sender).DataContext);
            host.PerformVncService();
        }

        private void btnOpenTelnet_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnOpenTelnet_Click()");
            Button button = (Button)sender;
            IServicableHost host = (IServicableHost)(button.DataContext);
            if (((BasicHost)host).IsShellMenuAvailable) {
                button.ContextMenu.IsOpen = true;
            } else {
                host.PerformShellService();
            }
        }

        private void btnOpenWeb_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("MainWindow.btnOpenWeb_Click()");
            Button button = (Button)sender;
            IServicableHost host = (IServicableHost)(button.DataContext);
            if (((BasicHost)host).IsWebMenuAvailable) {
                button.ContextMenu.IsOpen = true;
            } else {
                host.PerformWebService();
            }
        }

        private void btnRemoveGroup_Click(object sender, RoutedEventArgs e) {
            String groupName = (String)((Button)sender).Tag;
            System.Diagnostics.Debug.WriteLine("MainWindow.btnRemoveGroup_Click(): " + groupName);
            hostsManager.RemoveAllHostsInGroup(groupName);
        }

    }
}
