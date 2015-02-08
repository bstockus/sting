using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QuickSting {
    public class HostCollection : ObservableCollection<Host>, INotifyPropertyChanged {

        private bool isPaused = false;

        private Dictionary<string, bool> groupsDictionary = new Dictionary<string, bool>();

        public HostCollection()
            : base() {
            ICollectionView view = CollectionViewSource.GetDefaultView(this);
            view.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            view.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));
            this.CollectionChanged += HostCollection_CollectionChanged;
        }

        private void HostCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            foreach (Host host in e.NewItems) {
                if (!this.groupsDictionary.ContainsKey(host.GroupName)) {
                    if (host.GroupName.ToString().Equals("")) {
                        this.groupsDictionary.Add(host.GroupName.ToString(), true);
                    } else {
                        this.groupsDictionary.Add(host.GroupName.ToString(), false);
                    }
                }
            }
        }

        public bool IsPaused {
            get {
                return this.isPaused;
            }
            set {
                this.isPaused = value;
                this.NotifyPropertyChanged("IsPaused");
            }
        }

        public void GroupExpanded(string groupName) {
            this.groupsDictionary[groupName] = true;
        }

        public void GroupCollapsed(string groupName) {
            this.groupsDictionary[groupName] = false;
        }

        public void PingHosts(string siteName) {
            Task.Factory.StartNew(() => {
                foreach (Host host in this) {
                    if (this.groupsDictionary[host.GroupName]) {
                        host.Ping(!this.IsPaused, siteName).Start();
                    }
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void NotifyPropertyChanged(string propertyName) {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

    }
}
