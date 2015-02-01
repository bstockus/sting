using System;
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

namespace QuickSting {

    public struct HostInformation {

        public string Name { get; set; }

        public string GroupName { get; set; }

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
        private UIElement hostControls;

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

        public UIElement HostControls {
            get {
                return this.hostControls;
            }
            private set {
                this.hostControls = value;
                this.NotifyPropertyChanged("HostControls");
            }
        }

        public IPAddress IPAddress { get; private set; }

        public IExternalProgram[] ExternalPrograms { get; private set; }

        public Host(HostInformation hostInformation, IPAddress ipAddress) {
            this.Name = hostInformation.Name;
            this.GroupName = hostInformation.GroupName;
            this.HostStatus = QuickSting.HostStatus.Unknown;
            this.HostControls = new UIElement();
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

        public Task Ping() {
            Task task = new Task(() => {
                this.DoPing();
            });
            return task;
        }

        private void DoPing() {
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
