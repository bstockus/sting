using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sting.Host {
    public abstract class BasicHost : IPingableHost, IDisplayableHost, IControllableHost, IRemovableHost {

        private IPAddress ipAddress;
        protected Boolean paused;
        protected Boolean hostUp;
        protected String status;
        protected String details;

        public BasicHost(IPAddress ipAddress) {
            this.ipAddress = ipAddress;
        }

        public abstract string Title { get; }

        public abstract string SubTitle { get; }

        public string Status {
            get {
                return this.status;
            }
        }

        public string Details {
            get {
                return this.details;
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

        public void Pause() {
            this.paused = true;
        }

        public void UnPause() {
            this.paused = false;
        }

        public void RemoveHost() {
            // Nothing to do at this point
        }

        public void Ping() {
            throw new NotImplementedException();
        }

    }
}
