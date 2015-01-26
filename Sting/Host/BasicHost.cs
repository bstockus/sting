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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {

    public class BasicHostAllReadyExistsException : Exception {

    }

    public abstract class BasicHost : IPingableHost, IDisplayableHost, IControllableHost, IRemovableHost, INotifyPropertyChanged, IGroupableHost {

        private static List<IPAddress> currentHostIPAddresses = new List<IPAddress>();

        private static TimeSpan BAD_PING_STREAK_TIMEOUT {
            get {
                return new TimeSpan(0, 0, SettingsManager.GetSettings().BadPingStreakBeforeHostIsDown);
            }
        }

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

        protected Boolean vncServiceAvailable = false;
        protected Boolean shellServiceAvailable = false;
        protected Boolean webServiceAvailable = false;

        protected Boolean shellMenuAvailable = false;
        protected Boolean webMenuAvailable = false;

        public BasicHost(IPAddress ipAddress) {
            if (BasicHost.currentHostIPAddresses.Contains(ipAddress)) {
                System.Diagnostics.Debug.WriteLine("Currently a host open with the IP Address: " + ipAddress.ToString());
                throw new BasicHostAllReadyExistsException();
            } else {
                BasicHost.currentHostIPAddresses.Add(ipAddress);
                this.ipAddress = ipAddress;
                this.guid = Guid.NewGuid().ToString();
            }
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

        public virtual string NotificationTitle {
            get {
                return this.Title;
            }
        }

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
                return this.vncServiceAvailable && ServicesManager.GetServicesManager().IsVncServiceAvailable;
            }
        }

        public Boolean IsShellServiceAvailable {
            get {
                return this.shellServiceAvailable && ServicesManager.GetServicesManager().IsShellServiceAvailable;
            }
        }

        public Boolean IsWebServiceAvailable {
            get {
                return this.webServiceAvailable && ServicesManager.GetServicesManager().IsWebServiceAvailable;
            }
        }

        public Boolean IsShellMenuAvailable {
            get {
                return this.shellMenuAvailable && this.IsShellServiceAvailable;
            }
        }

        public Boolean IsWebMenuAvailable {
            get {
                return this.webMenuAvailable && this.IsWebServiceAvailable;
            }
        }

        public void RemoveHost() {
            BasicHost.currentHostIPAddresses.Remove(this.ipAddress);
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
                    NotificationManager.GetNotificationManager().Notify(this.NotificationTitle, "Host is Up!", "green_up_arrow.png");
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
                        NotificationManager.GetNotificationManager().Notify(this.NotificationTitle, "Host is Down!", "red_down_arrow.png");
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


        private static Boolean[] IsInTimeSpans(DateTime[] timeSpans, DateTime dateTime) {
            Boolean[] results = new Boolean[timeSpans.Length];
            for (int index = 0; index < timeSpans.Length; index++) {
                if (dateTime > timeSpans[index]) {
                    results[index] = true;
                } else {
                    results[index] = false;
                }
            }
            return results;
        }

        private static int[] AddValueIfInTimeSpan(Boolean[] bools, int[] inputs, int value) {
            int[] results = inputs;
            for (int index = 0; index < bools.Length; index++) {
                if (bools[index]) {
                    results[index] += value;
                }
            }
            return results;
        }

        private static int[] SetValueIfInTimeSpanAndMax(Boolean[] bools, int[] inputs, int value) {
            int[] results = inputs;
            for (int index = 0; index < bools.Length; index++) {
                if (bools[index] && value > inputs[index]) {
                    results[index] = value;
                }
            }
            return results;
        }

        private static int[] SetValueIfInTimeSpanAndMin(Boolean[] bools, int[] inputs, int value) {
            int[] results = inputs;
            for (int index = 0; index < bools.Length; index++) {
                if (bools[index] && value < inputs[index]) {
                    results[index] = value;
                }
            }
            return results;
        }

        private void UpdateDetailsForHostUp() {

            DateTime minuteAgo = DateTime.Now - new TimeSpan(0, 1, 0);
            DateTime fiveMinutesAgo = DateTime.Now - new TimeSpan(0, 5, 0);
            DateTime tenMinutesAgo = DateTime.Now - new TimeSpan(0, 10, 0);
            var lastTenMinutesPingReplys = from tuple in this.pingReplyHistory
                                       where tuple.Item1 > tenMinutesAgo
                                       select tuple;

            DateTime[] agos = new DateTime[] { minuteAgo, fiveMinutesAgo, tenMinutesAgo };

            int[] latencySums = new int[] { 0, 0, 0 };
            int[] maxLatencys = new int[] { 0, 0, 0 };
            int[] minLatencys = new int[] { 0, 0, 0 };

            int[] totalPings = new int[] { 0, 0, 0 };
            int[] badPings = new int[] { 0, 0, 0 };

            Boolean first = true;
            foreach (var tuple in lastTenMinutesPingReplys) {
                Boolean[] inTimeSpans = IsInTimeSpans(agos, tuple.Item1);
                totalPings = AddValueIfInTimeSpan(inTimeSpans, totalPings, 1);

                if (tuple.Item2.Status == IPStatus.Success) {
                    int rtTime = (int)tuple.Item2.RoundtripTime;
                    if (first) {
                        minLatencys = AddValueIfInTimeSpan(inTimeSpans, minLatencys, rtTime);
                        first = false;
                    }
                    maxLatencys = SetValueIfInTimeSpanAndMax(inTimeSpans, maxLatencys, rtTime);
                    minLatencys = SetValueIfInTimeSpanAndMin(inTimeSpans, minLatencys, rtTime);
                    latencySums = AddValueIfInTimeSpan(inTimeSpans, latencySums, rtTime);
                } else {
                    badPings = AddValueIfInTimeSpan(inTimeSpans, badPings, 1);
                }
            }

            String lastMinuteAverageLatency = (latencySums[0] / (totalPings[0] - badPings[0])).ToString();
            String lastFiveMinutesAverageLatency = (latencySums[1] / (totalPings[1] - badPings[1])).ToString();
            String lastTenMinutesAverageLatency = (latencySums[2] / (totalPings[2] - badPings[2])).ToString();

            String lastMinuteMaxLatency = maxLatencys[0].ToString();
            String lastFivMinutesMaxLatency = maxLatencys[1].ToString();
            String lastTenMinutesMaxLatency = maxLatencys[2].ToString();

            String badPingsTooltip = "";
            if (badPings[0] > 0) {
                String lastMinuteBadPings = ((float)badPings[0] / (float)totalPings[0]).ToString("F2");
                String lastFiveMinutesBadPings = ((float)badPings[1] / (float)totalPings[1]).ToString("F2");
                String lastTenMinutesBadPings = ((float)badPings[2] / (float)totalPings[2]).ToString("F2");

                badPingsTooltip = System.Environment.NewLine + "Bad = ";

                if (totalPings[2] > totalPings[1]) {
                    badPingsTooltip += lastMinuteBadPings + "/" + lastFiveMinutesBadPings + "/" + lastTenMinutesBadPings;
                } else if (totalPings[1] > totalPings[0]) {
                    badPingsTooltip += lastMinuteBadPings + "/" + lastFiveMinutesBadPings;
                } else {
                    badPingsTooltip += lastMinuteBadPings;
                }

            }
            
            String maxLatenciesTooltip = "";
            
            if (totalPings[2] > totalPings[1]) {
                this.details = lastMinuteAverageLatency + "/" + lastFiveMinutesAverageLatency + "/" + lastTenMinutesAverageLatency + "ms";
                maxLatenciesTooltip = lastMinuteMaxLatency + "/" + lastFivMinutesMaxLatency + "/" + lastMinuteMaxLatency + "ms";
            } else if (totalPings[1] > totalPings[0]) {
                this.details = lastMinuteAverageLatency + "/" + lastFiveMinutesAverageLatency + "ms";
                maxLatenciesTooltip = lastMinuteMaxLatency + "/" + lastFivMinutesMaxLatency + "ms";
            } else {
                this.details = lastMinuteAverageLatency +  "ms";
                maxLatenciesTooltip = lastMinuteMaxLatency + "ms";
            }

            this.detailsTooltip = "Max = " + maxLatenciesTooltip + badPingsTooltip;
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
