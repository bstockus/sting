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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Sting {
    public sealed class NotificationManager {

        private static NotificationManager singleton = null;

        public static NotificationManager GetNotificationManager() {
            if (singleton == null) {
                singleton = new NotificationManager();
            }
            return singleton;
        }

        private List<NotificationWindow> notificationWindows = new List<NotificationWindow>();

        private int currentHighId = 0;
        private int currentLowId = 0;

        public NotificationManager() {
            if (NotificationManager.singleton != null) {
                throw new Exception("Trying to create another instance of NotificationManager. This class is a singleton. Come on Son!");
            }
        }

        public void Notify(string title, string message) {
            Task.Run(() => {
                NotificationWindow notificationWindow;
                App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                    notificationWindow = new NotificationWindow(title, message, currentHighId++);
                    notificationWindow.Show();
                }));
            });
        }

        public void UpdateWindowPositions(double actualHeight, NotificationWindow notificationWindow, int id) {

            while (id < currentLowId) {

            }

            if (this.notificationWindows.Count > 0) {
                // We Will have to Push all the windows Up
                foreach (NotificationWindow window in notificationWindows) {
                    window.Top -= actualHeight;
                }
            }

            currentLowId++;

            Task.Run(() => {
                this.notificationWindows.Add(notificationWindow);
                Thread.Sleep(TimeSpan.FromSeconds(10));
                this.notificationWindows.Remove(notificationWindow);
                App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                    notificationWindow.Close();
                }));
            });
        }

    }
}
