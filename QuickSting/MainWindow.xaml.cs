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

        private Dictionary<string, bool> groupsDictionary = new Dictionary<string, bool>();

        private bool paused = false;

        private IntPtr windowHandle;

        public String SiteName { get; set; }

        public bool IsPaused {
            get {
                return this.paused;
            }
            set {
                this.paused = value;
                this.NotifyPropertyChanged("IsPaused");
            }
        }

        public MainWindow(String title, HostCollection hostCollection) {
            this.HostCollection = hostCollection;
            this.SiteName = title;
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
            this.pingDispatchTimer.Stop();
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

            foreach (CollectionViewGroup group in view.Groups) {
                if (group.Name.ToString().Equals("")) {
                    this.groupsDictionary.Add(group.Name.ToString(), true);
                } else {
                    this.groupsDictionary.Add(group.Name.ToString(), false);
                }
            }

            this.pingDispatchTimer.Tick += new EventHandler(pingDispatchTimer_Tick);
            this.pingDispatchTimer.Interval = new TimeSpan(0, 0, 2);
            this.pingDispatchTimer.Start();

            this.NotifyPropertyChanged("SiteName");
            this.Title = this.SiteName;
            this.NotifyPropertyChanged("Title");

            this.windowHandle = new WindowInteropHelper(this).Handle;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Win32Interop.ReleaseCapture();
            Win32Interop.SendMessage(this.windowHandle, 0xA1, (IntPtr)0x2, (IntPtr)0);
        }

        private void pingDispatchTimer_Tick(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("pingDispatchTimer_Tick");
            Task.Factory.StartNew(() => {
                foreach (Host host in this.HostCollection) {
                    if (this.groupsDictionary[host.GroupName]) {
                        host.Ping(!this.IsPaused, this.SiteName).Start();
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

        private void Expander_Collapsed(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("Expander_Collapsed");
            Expander expander = (Expander)e.Source;
            CollectionViewGroup collectionViewGroup = (CollectionViewGroup)expander.DataContext;
            System.Diagnostics.Debug.WriteLine(collectionViewGroup.Name);
            this.groupsDictionary[collectionViewGroup.Name.ToString()] = false;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("Expander_Expanded");
            Expander expander = (Expander)e.Source;
            CollectionViewGroup collectionViewGroup = (CollectionViewGroup)expander.DataContext;
            System.Diagnostics.Debug.WriteLine(collectionViewGroup.Name);
            this.groupsDictionary[collectionViewGroup.Name.ToString()] = true;
        }

        private void btnNotify_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("btnNotify_Click");
            this.IsPaused = !(this.IsPaused);
            this.NotifyPropertyChanged("IsPaused");
        }

        private void btnSiteDetails_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("btnSiteDetails_Click");
            this.ctxSiteDetails.IsOpen = true;
        }

        private void ctxSiteDetails_StorePictures_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("ctxSiteDetails_StorePictures_Click");
        }

        private void ctxSiteDetails_CallStore_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("ctxSiteDetails_CallStore_Click");
        }

        private void ctxSiteDetails_EmailStore_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("ctxSiteDetails_EmailStore_Click");
        }

    }
}
