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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sting {
    public class Host {

        private IPAddress ipAddress = null;
        private String dnsName = null;

        public String Name {
            get {
                if (this.dnsName != null) {
                    return this.DNSName;
                } else {
                    return this.IPAddress.ToString();
                }
            }
        }

        public String SubName {
            get {
                if (this.dnsName != null) {
                    return this.IPAddress.ToString();
                } else {
                    return "";
                }
            }
        }

        public String DNSName {
            get {
                if (this.dnsName != null) {
                    return this.dnsName;
                } else {
                    return "";
                }
            }
            set {
                this.dnsName = value;
            }
        }

        public IPAddress IPAddress {
            get {
                return this.ipAddress;
            }
        }

        public String StatusText {
            get {
                if (this.Active) {
                    TimeSpan ts = DateTime.Now - this.HostStateTransitionTime;
                    String timeSpan = "";
                    if (ts.TotalSeconds < 60) {
                        timeSpan = ts.ToString("%s's'");
                    } else if (ts.TotalMinutes < 60) {
                        timeSpan = ts.ToString("%m'm '%s's'");
                    } else {
                        timeSpan = ts.ToString("%h'h '%m'm '%s's'");
                    }
                    if (this.HostIsUp) {
                        return "Up for " + timeSpan;
                    } else {
                        return "Down for " + timeSpan;
                    }
                } else {
                    return "Paused";
                }
            }
        }

        public int PingInterval { get; set; }

        private Boolean active = false;

        public Boolean Active {
            get {
                return this.active;
            }
            set {
                if (this.active != value) {
                    this.HostStateTransitionTime = DateTime.Now;
                }
                this.active = value;
            }
        }

        private Boolean hostState;

        public Boolean HostIsUp {
            get {
                return this.hostState;
            }
            private set {
                if (this.hostState != value) {
                    this.HostStateTransitionTime = DateTime.Now;
                }
                this.hostState = value;
            }
        }

        public DateTime HostStateTransitionTime { get; private set; }

        public String GUID { get; private set; }

        public float Latency { get; private set; }

        public Thread PingThread { get; private set; }

        public Host(IPAddress ipAddress, int pingInterval, bool active) {
            this.GUID = Guid.NewGuid().ToString();
            this.ipAddress = ipAddress;
            this.PingThread = new Thread(new ThreadStart(PingProc));
            this.PingThread.Start();
            this.PingInterval = pingInterval;
            this.Active = active;
        }

        private int failedPingCounter = 0;
        DateTime lastPingTime = DateTime.Now;

        public void PingProc() {
            try {
                if (this.Active) {
                    PerformPing();
                }
                while (true) {
                    System.Diagnostics.Debug.WriteLine("Thread " + Thread.CurrentThread.Name + ": Host=" + this.GUID + ", IP=" + this.IPAddress.ToString());

                    if (this.Active) {
                        if ((lastPingTime + new TimeSpan(0, 0, this.PingInterval) < DateTime.Now )) {
                            PerformPing();
                        }
                    }

                    Thread.Sleep(1000);
                }
            } catch (ThreadAbortException e) {
                return;
            } catch (PingException e) {
                return;
            }
        }

        private void PerformPing() {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send(this.ipAddress, timeout, buffer, options);
            lastPingTime = DateTime.Now;
            if (reply.Status == IPStatus.Success) {
                System.Diagnostics.Debug.WriteLine(this.IPAddress.ToString() + ": " + reply.ToString());
                failedPingCounter = 0;
                this.HostIsUp = true;
            } else {
                failedPingCounter++;
                if (failedPingCounter > 5) {
                    this.HostIsUp = false;
                }
            }
        }

        public void Terminate() {
            this.PingThread.Abort();
        }

    }
}
