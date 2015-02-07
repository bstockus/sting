using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSting {
    public class HostCollection : ObservableCollection<Host> {

        private bool isPaused = false;

        public bool IsPaused {
            get {
                return this.isPaused;
            }
            set {
                this.isPaused = value;
            }
        }



    }
}
