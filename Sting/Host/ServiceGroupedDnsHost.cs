﻿// Copyright 2014-2015, Bryan Stockus
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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sting.Host {

    public class ServiceGroupedDnsHost : GroupedDnsHost, IServicableHost {

        private VncHostParams vncHostParams;
        private WebHostParams[] webHostParams;

        public void WebContextMenuItem_Click(Object sender, RoutedEventArgs e) {
            WebHostParams webHostParam = (WebHostParams)(((MenuItem)sender).Tag);
            e.Handled = true;
            System.Diagnostics.Debug.WriteLine("Web Host Params Name=" + webHostParam.Name);
            ServicesManager.GetServicesManager().LaunchWebServiceForHost(this.IPAddress, webHostParam);
        }

        public ContextMenu ShellContextMenu {
            get {
                return null;
            }
        }

        public ContextMenu WebContextMenu {
            get {
                ContextMenu contextMenu = new ContextMenu();
                foreach (WebHostParams webHostParam in this.webHostParams) {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = webHostParam.Name;
                    menuItem.Tag = webHostParam;
                    menuItem.Click += this.WebContextMenuItem_Click;
                    contextMenu.Items.Add(menuItem);
                }
                return contextMenu;
            }
        }

        public ServiceGroupedDnsHost(IPAddress ipAddress, String domainName, String groupName, VncHostParams vncHostParams, WebHostParams[] webHostParams)
            : base(ipAddress, domainName, groupName) {
                this.vncHostParams = vncHostParams;
                if (vncHostParams != null) {
                    this.vncServiceAvailable = true;
                }
                this.webHostParams = webHostParams;
                if (webHostParams != null) {
                    if (webHostParams.Length > 0) {
                        this.webServiceAvailable = true;
                        if (webHostParams.Length > 1) {
                            this.webMenuAvailable = true;
                        }
                    }
                }
        }

        public void PerformVncService() {
            ServicesManager.GetServicesManager().LaunchVncServiceForHost(this.IPAddress, this.vncHostParams);
        }

        public void PerformShellService() {

        }

        public void PerformWebService() {
            ServicesManager.GetServicesManager().LaunchWebServiceForHost(this.IPAddress, webHostParams[0]);
        }

    }

}
