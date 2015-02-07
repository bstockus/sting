﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace QuickSting {

    public struct HostInformation {

        public string Name { get; set; }

        public string GroupName { get; set; }

        public Service[] Services { get; set; }

    }

    [Serializable]
    public struct Service {

        [XmlAttribute("Type")]
        public string ServiceType { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Program")]
        public string Program { get; set; }

        [XmlText]
        public string CommandLine { get; set; }

    }

    public enum HostStatus : int {
        Up = 1,
        Down,
        Bad,
        Unknown
    }

    public class Host : INotifyPropertyChanged {

        private String name;
        private String groupName;
        private HostStatus hostStatus;
        private ContextMenu hostControls;
        private UIElement hostToolTip;

        public String Name {
            get {
                return this.name;
            }
            private set {
                this.name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        public String GroupName {
            get {
                return this.groupName;
            }
            private set {
                this.groupName = value;
                this.NotifyPropertyChanged("GroupName");
            }
        }

        public HostStatus HostStatus {
            get {
                return this.hostStatus;
            }
            private set {
                this.hostStatus = value;
                this.NotifyPropertyChanged("HostStatus");
                this.NotifyPropertyChanged("HostStatusCode");
            }
        }

        public int HostStatusCode {
            get {
                return (int)this.hostStatus;
            }
        }

        public ContextMenu HostControls {
            get {
                return this.hostControls;
            }
            private set {
                this.hostControls = value;
                this.NotifyPropertyChanged("HostControls");
            }
        }

        public UIElement HostToolTip {
            get {
                return this.hostToolTip;
            }
            private set {
                this.hostToolTip = value;
                this.NotifyPropertyChanged("HostToolTip");
            }
        }

        public IPAddress IPAddress { get; private set; }

        public IExternalProgram[] ExternalPrograms { get; private set; }

        public Host(HostInformation hostInformation, IPAddress ipAddress) {
            this.Name = hostInformation.Name;
            this.GroupName = hostInformation.GroupName;
            this.HostStatus = QuickSting.HostStatus.Unknown;

            ContextMenu contextMenu = new ContextMenu();
            if (hostInformation.Services != null) {
                foreach (Service service in hostInformation.Services) {
                    MenuItem menuItem = new MenuItem();
                    if (service.ServiceType.Equals("VNC")) {
                        menuItem.Icon = new Image {
                            Source = new BitmapImage(new Uri("pack://application:,,,/QuickSting;component/Images/svc_vnc.png"))
                        };
                    } else if (service.ServiceType.Equals("CMD")) {
                        menuItem.Icon = new Image {
                            Source = new BitmapImage(new Uri("pack://application:,,,/QuickSting;component/Images/svc_cmd_line.png"))
                        };
                    } else if (service.ServiceType.Equals("WEB")) {
                        menuItem.Icon = new Image {
                            Source = new BitmapImage(new Uri("pack://application:,,,/QuickSting;component/Images/svc_web.png"))
                        };
                    } else {
                        menuItem.Icon = new Image {
                            Source = new BitmapImage(new Uri("pack://application:,,,/QuickSting;component/Images/svc_blank.png"))
                        };
                    }
                    
                    menuItem.Header = service.Name;
                    menuItem.Tag = service;
                    menuItem.Click += this.ServicesMenuItem_Click;
                    contextMenu.Items.Add(menuItem);
                }
            } else {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = "<No Services>";
                menuItem.IsEnabled = false;
                contextMenu.Items.Add(menuItem);
            }

            Border border = new Border();
            border.Margin = new Thickness(0.0);
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            border.BorderThickness = new Thickness(1.0);
            border.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            Grid grid = new Grid();

            ColumnDefinition columnDefinition1 = new ColumnDefinition();

            RowDefinition rowDefinition1 = new RowDefinition();

            grid.ColumnDefinitions.Add(columnDefinition1);
            grid.RowDefinitions.Add(rowDefinition1);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = ipAddress.ToString();
            Grid.SetColumn(grid, 0);
            Grid.SetRow(grid, 0);
            grid.Children.Add(textBlock);

            border.Child = (UIElement)grid;

            this.HostToolTip = (UIElement)border;

            this.HostControls = contextMenu;
            this.IPAddress = ipAddress;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void NotifyPropertyChanged(string propertyName) {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public void ServicesMenuItem_Click(Object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("ServicesMenuItem_Click");
            MenuItem menuItem = (MenuItem)e.Source;
            Service service = (Service)menuItem.Tag;
            System.Diagnostics.Debug.WriteLine(service.CommandLine);
            string commandLine = service.CommandLine;
            if (commandLine == null) commandLine = "";
            commandLine = commandLine.Replace("%%%IP%%%", this.IPAddress.ToString());
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.Arguments = commandLine;
            startInfo.FileName = service.Program;
            System.Diagnostics.Process.Start(startInfo);
        }

        #region Ping Handler

        private static TimeSpan BAD_PING_STREAK_TIMEOUT {
            get {
                return new TimeSpan(0, 0, 10);
            }
        }

        private DateTime lastStatusChangeTime = DateTime.Now;
        private DateTime firstBadPingTime = DateTime.Now;
        private Boolean isInBadPingStreak = false;

        private List<Tuple<DateTime, PingReply>> pingReplyHistory = new List<Tuple<DateTime, PingReply>>();

        public Task Ping(bool notify) {
            Task task = new Task(() => {
                this.DoPing(notify);
            });
            return task;
        }

        private void DoPing(bool notify) {
            System.Diagnostics.Debug.WriteLine("Sent Ping to " + this.IPAddress.ToString());
            Ping ping = new Ping();
            PingOptions pingOptions = new PingOptions();

            pingOptions.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            PingReply reply = ping.Send(this.IPAddress, 120, buffer, pingOptions);

            this.pingReplyHistory.Add(new Tuple<DateTime, PingReply>(DateTime.Now, reply));

            if (reply.Status == IPStatus.Success) {
                if (this.isInBadPingStreak) {
                    this.isInBadPingStreak = false;
                }
                if (this.HostStatus != QuickSting.HostStatus.Up) {
                    this.lastStatusChangeTime = DateTime.Now;
                    //NotificationManager.GetNotificationManager().Notify(this.NotificationTitle, "Host is Up!", "green_up_arrow.png");
                    this.OnPropertyChanged(new PropertyChangedEventArgs("IsHostUp"));
                }
                this.HostStatus = QuickSting.HostStatus.Up;
                System.Diagnostics.Debug.WriteLine("Good Ping from " + this.IPAddress.ToString());

            } else {
                if (this.HostStatus != QuickSting.HostStatus.Down) {
                    if (!this.isInBadPingStreak) {
                        this.isInBadPingStreak = true;
                        this.firstBadPingTime = DateTime.Now;
                        if (this.HostStatus == QuickSting.HostStatus.Unknown) {
                            this.HostStatus = QuickSting.HostStatus.Down;
                        } else {
                            this.HostStatus = QuickSting.HostStatus.Bad;
                        }
                    } else if (DateTime.Now - this.firstBadPingTime > BAD_PING_STREAK_TIMEOUT) {
                        this.lastStatusChangeTime = DateTime.Now;
                        this.HostStatus = QuickSting.HostStatus.Down;
                        //NotificationManager.GetNotificationManager().Notify(this.NotificationTitle, "Host is Down!", "red_down_arrow.png");
                        this.OnPropertyChanged(new PropertyChangedEventArgs("IsHostUp"));
                    }
                }
                System.Diagnostics.Debug.WriteLine("Bad Ping from " + this.IPAddress.ToString());
            }

        }

        #endregion

    }
}