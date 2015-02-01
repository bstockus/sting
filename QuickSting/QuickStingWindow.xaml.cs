using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace QuickSting {
    /// <summary>
    /// Interaction logic for QuickStingWindow.xaml
    /// </summary>
    public partial class QuickStingWindow : Window {

        KeyboardHook hook = new KeyboardHook();

        public QuickStingWindow() {
            InitializeComponent();

            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            hook.RegisterHotKey(ModifierKeys.Alt | ModifierKeys.Win, Keys.P);

        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("hook_KeyPressed!");
            this.ToggleVisibility();
        }

        private void Window_Activated(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("Window_Activated");
            txtSearch.Focus();
        }

        private void Window_Deactivated(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("Window_Deactivated");
            this.HideWindow();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("btnClose_Click");
            this.HideWindow();
        }

        private void HideWindow() {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ShowWindow() {
            this.Activate();
            this.Visibility = System.Windows.Visibility.Visible;
        }

        private void ToggleVisibility() {
            if (this.Visibility == System.Windows.Visibility.Hidden) {
                this.ShowWindow();
            } else {
                this.HideWindow();
            }
        }

        private void txtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            System.Diagnostics.Debug.WriteLine("txtSearch_KeyDown");
            if (e.Key == Key.Escape) {
                this.HideWindow();
                e.Handled = true;
            } else if (e.Key == Key.Enter) {
                e.Handled = true;
            }
        }

    }
}
