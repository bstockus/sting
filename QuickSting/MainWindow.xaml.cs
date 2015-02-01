using System;
using System.Collections.Generic;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickSting {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        public HostCollection HostCollection { get; private set; }

        private bool paused = false;

        public bool IsPaused {
            get {
                return this.paused;
            }
            set {
                this.paused = value;
                this.NotifyPropertyChanged("IsPaused");
            }
        }

        public MainWindow() {
            this.HostCollection = new HostCollection();
            this.HostCollection.Add(new Host(new HostInformation {
                Name = "Host 1",
                GroupName = ""
            }, IPAddress.Parse("10.0.1.1")));
            this.HostCollection.Add(new Host(new HostInformation {
                Name = "Host 2",
                GroupName = ""
            }, IPAddress.Parse("10.0.1.201")));
            this.HostCollection.Add(new Host(new HostInformation {
                Name = "Host 3",
                GroupName = ""
            }, IPAddress.Parse("10.0.1.55")));
            this.HostCollection.Add(new Host(new HostInformation {
                Name = "Host 4",
                GroupName = "Group 1"
            }, IPAddress.Parse("8.8.8.8")));
            InitializeComponent();
        }

        private System.Windows.Threading.DispatcherTimer pingDispatchTimer = new System.Windows.Threading.DispatcherTimer();



        //Attach this to the PreviewMousLeftButtonDown event of the grip control in the lower right corner of the form to resize the window
        private void WindowResize(object sender, MouseButtonEventArgs e) {
            HwndSource hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            Win32Interop.SendMessage(hwndSource.Handle, 0x112, (IntPtr)61448, IntPtr.Zero);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("btnClose_Click");
            this.Close();
            e.Handled = true;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("btnMinimize_Click");
            this.WindowState = WindowState.Minimized;
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("Window_Loaded");
            this.lstHosts.ItemsSource = this.HostCollection;

            ICollectionView view = CollectionViewSource.GetDefaultView(lstHosts.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            view.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));

            this.pingDispatchTimer.Tick += new EventHandler(pingDispatchTimer_Tick);
            this.pingDispatchTimer.Interval = new TimeSpan(0, 0, 2);
            this.pingDispatchTimer.Start();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Win32Interop.ReleaseCapture();
            Win32Interop.SendMessage(new WindowInteropHelper(this).Handle, 0xA1, (IntPtr)0x2, (IntPtr)0);
        }

        private void pingDispatchTimer_Tick(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("pingDispatchTimer_Tick");
            if (!this.IsPaused) {
                Task.Factory.StartNew(() => {
                    foreach (Host host in this.HostCollection) {
                        host.Ping().Start();
                    }
                });
            }
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
