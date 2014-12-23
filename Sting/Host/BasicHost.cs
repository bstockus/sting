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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {
    public abstract class BasicHost : IPingableHost, IDisplayableHost, IControllableHost, IRemovableHost, INotifyPropertyChanged, IGroupableHost {

        private static TimeSpan BAD_PING_STREAK_TIMEOUT = new TimeSpan(0, 0, 10);

        private IPAddress ipAddress;
        protected Boolean paused;
        protected Boolean hostUp;
        protected String status;
        protected String details;
        protected String groupName = "";
        private String guid;

        private String detailsTooltip;

        private DateTime lastStatusChangeTime = DateTime.Now;
        private DateTime firstBadPingTime = DateTime.Now;
        private Boolean isInBadPingStreak = false;

        private List<Tuple<DateTime, PingReply>> pingReplyHistory = new List<Tuple<DateTime, PingReply>>();

        public BasicHost(IPAddress ipAddress) {
            this.ipAddress = ipAddress;
            this.guid = Guid.NewGuid().ToString();
        }

        public string GUID {
            get {
                return this.guid;
            }
        }

        public string GroupName {
            get {
                return this.groupName;
            }
        }

        public abstract string Title { get; }

        public abstract string SubTitle { get; }

        public string Status {
            get {
                if (this.IsPaused) {
                    return "Paused";
                } else {
                    TimeSpan ts = DateTime.Now - this.lastStatusChangeTime;
                    String timeSpan = "";
                    if (ts.TotalSeconds < 60) {
                        timeSpan = ts.ToString("%s's'");
                    } else if (ts.TotalMinutes < 60) {
                        timeSpan = ts.ToString("%m'm '%s's'");
                    } else {
                        timeSpan = ts.ToString("%h'h '%m'm '%s's'");
                    }
                    return timeSpan;
                }
            }
        }

        public string Details {
            get {
                if (!this.IsPaused) {
                    return this.details;
                } else {
                    return "";
                }
            }
        }

        public string DetailsTooltip {
            get {
                if (!this.IsPaused) {
                    return this.detailsTooltip;
                } else {
                    return "";
                }
            }
        }

        public bool IsPaused {
            get {
                return this.paused;
            }
        }

        public bool IsHostUp {
            get {
                return this.hostUp;
            }
        }

        public IPAddress IPAddress {
            get {
                return this.ipAddress;
            }
        }

        public void Pause() {
            this.paused = true;
            this.lastStatusChangeTime = DateTime.Now;
            this.OnPropertyChanged(new PropertyChangedEventArgs("IsPaused"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Details"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Status"));
        }

        public void UnPause() {
            this.paused = false;
            this.lastStatusChangeTime = DateTime.Now;
            this.isInBadPingStreak = false;
            this.OnPropertyChanged(new PropertyChangedEventArgs("IsPaused"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Details"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Status"));
        }

        public Boolean IsVncServiceAvailable {
            get {
                return true;
            }
        }

        public Boolean IsShellServiceAvailable {
            get {
                return true;
            }
        }

        public Boolean IsWebServiceAvailable {
            get {
                return true;
            }
        }

        public void RemoveHost() {
            // Nothing to do at this point
        }

        public Task Ping() {
            Task task = new Task(() => {
                if (!this.IsPaused) {
                    this.DoPing();
                }
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
                if (!this.hostUp) {
                    this.lastStatusChangeTime = DateTime.Now;
                    this.hostUp = true;
                    NotificationManager.GetNotificationManager().Notify(this.Title, "Host is Up!", "green_up_arrow.png");
                    this.OnPropertyChanged(new PropertyChangedEventArgs("IsHostUp"));
                }
                System.Diagnostics.Debug.WriteLine("Good Ping from " + this.IPAddress.ToString());

            } else {
                if (this.IsHostUp) {
                    if (!this.isInBadPingStreak) {
                        this.isInBadPingStreak = true;
                        this.firstBadPingTime = DateTime.Now;
                    } else if (DateTime.Now - this.firstBadPingTime > BAD_PING_STREAK_TIMEOUT) {
                        this.lastStatusChangeTime = DateTime.Now;
                        this.hostUp = false;
                        NotificationManager.GetNotificationManager().Notify(this.Title, "Host is Down!", "red_down_arrow.png");
                        this.OnPropertyChanged(new PropertyChangedEventArgs("IsHostUp"));
                    }
                }
                System.Diagnostics.Debug.WriteLine("Bad Ping from " + this.IPAddress.ToString());
            }

            if (this.IsHostUp) {
                this.UpdateDetailsForHostUp();
            } else {
                this.UpdateDetailsForHostDown();
            }
            
        }

        private void UpdateDetailsForHostUp() {
            DateTime minuteAgo = DateTime.Now - new TimeSpan(0, 1, 0);
            DateTime fiveMinutesAgo = DateTime.Now - new TimeSpan(0, 5, 0);
            var lastMinutePingReplys = from tuple in this.pingReplyHistory
                                       where tuple.Item1 > minuteAgo
                                       select tuple.Item2;
            int sum = 0;
            int count = 0;
            int max = 0;
            int min = 0;

            int totalPings = 0;
            int badPings = 0;

            Boolean first = true;
            foreach (PingReply pingReply in lastMinutePingReplys) {
                totalPings++;
                if (pingReply.Status == IPStatus.Success) {
                    int rtTime = (int)pingReply.RoundtripTime;
                    if (first) {
                        min = rtTime;
                        first = false;
                    }
                    if (rtTime < min) min = rtTime;
                    if (rtTime > max) max = rtTime;
                    sum += rtTime;
                    count++;
                } else {
                    badPings++;
                }
            }

            float badPingPercentage = ((float)badPings / (float)totalPings) * 100.0f;

            this.details = (sum / count).ToString() + "ms";
            this.detailsTooltip = "Min = " + min.ToString() + "ms, Max = " + max.ToString() + "ms" + System.Environment.NewLine
                + "Bad Pings = " + badPings.ToString() + "/" + totalPings.ToString() + " (" + badPingPercentage.ToString("F2") + "%)";
            this.OnPropertyChanged(new PropertyChangedEventArgs("Details"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("DetailsTooltip"));
        }

        private void UpdateDetailsForHostDown() {
            if (this.pingReplyHistory.Count > 0) {
                PingReply lastPingReply = this.pingReplyHistory[this.pingReplyHistory.Count - 1].Item2;
                this.details = System.Enum.GetName(typeof(IPStatus), lastPingReply.Status);
                if (lastPingReply.Address != null) {
                    this.detailsTooltip = "Reply from " + lastPingReply.Address.ToString();
                } else {
                    this.detailsTooltip = "";
                }
            } else {
                this.details = "";
                this.detailsTooltip = "";
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs("Details"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("DetailsTooltip"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }
}
