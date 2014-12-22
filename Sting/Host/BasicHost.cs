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
    public abstract class BasicHost : IPingableHost, IDisplayableHost, IControllableHost, IRemovableHost, INotifyPropertyChanged {

        private static int BAD_PING_ATTEMPTS_ALLOWED_BEFORE_DOWN = 4;

        private IPAddress ipAddress;
        protected Boolean paused;
        protected Boolean hostUp;
        protected String status;
        protected String details;
        private String guid;

        private DateTime lastStatusChangeTime = DateTime.Now;
        private int badPingAttemptCounter = 0;

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
                if (!this.IsPaused && this.hostUp) {
                    return this.details;
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
                Ping ping = new Ping();
                PingOptions pingOptions = new PingOptions();

                pingOptions.DontFragment = true;
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                PingReply reply = ping.Send(this.IPAddress, 120, buffer, pingOptions);

                this.pingReplyHistory.Add(new Tuple<DateTime, PingReply>(DateTime.Now, reply));

                this.details = this.CalculateLatencyAverage();
                this.OnPropertyChanged(new PropertyChangedEventArgs("Details"));

                if (reply.Status == IPStatus.Success) {
                    if (!this.hostUp) {
                        this.lastStatusChangeTime = DateTime.Now;
                        this.hostUp = true;
                        NotificationManager.GetNotificationManager().Notify(this.Title, "Host is Up!", "green_up_arrow.png");
                        this.OnPropertyChanged(new PropertyChangedEventArgs("IsHostUp"));
                    }
                    this.badPingAttemptCounter = 0;
                    System.Diagnostics.Debug.WriteLine("Good Ping from " + this.IPAddress.ToString());
                } else {
                    this.badPingAttemptCounter++;
                    if (this.badPingAttemptCounter >= BAD_PING_ATTEMPTS_ALLOWED_BEFORE_DOWN && this.hostUp) {
                        this.lastStatusChangeTime = DateTime.Now;
                        this.hostUp = false;
                        NotificationManager.GetNotificationManager().Notify(this.Title, "Host is Down!", "red_down_arrow.png");
                        this.OnPropertyChanged(new PropertyChangedEventArgs("IsHostUp"));
                    }
                    System.Diagnostics.Debug.WriteLine("Bad Ping from " + this.IPAddress.ToString());
                }

            });
            return task;
        }

        private string CalculateLatencyAverage() {
            DateTime minuteAgo = DateTime.Now - new TimeSpan(0, 1, 0);
            var lastMinutePingReplys = from tuple in this.pingReplyHistory
                                       where tuple.Item1 > minuteAgo
                                       select tuple.Item2;
            int sum = 0;
            int count = 0;
            int max = 0;
            int min = 0;
            Boolean first = true;
            foreach (PingReply pingReply in lastMinutePingReplys) {
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
                }
            }
            this.OnPropertyChanged(new PropertyChangedEventArgs("Details"));
            return (sum / count).ToString() + "ms";
        }

        private string GetHostDownReason() {
            if (this.pingReplyHistory.Count > 0) {
                PingReply lastPingReply = this.pingReplyHistory[this.pingReplyHistory.Count - 1].Item2;
                return System.Enum.GetName(typeof(IPStatus), lastPingReply.Status);
            } else {
                return "";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }
}
