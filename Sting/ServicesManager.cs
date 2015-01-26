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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sting {

    public class UnableToRunServiceException : Exception {

    }

    [Serializable]
    public class VncHostParams {

        [XmlAttribute]
        public String Password { get; set; }

        [XmlAttribute]
        public String UserName { get; set; }

        [XmlAttribute]
        public String Port { get; set; }

        [XmlAttribute]
        public String Custom1 { get; set; }

    }

    [Serializable]
    public class WebHostParams {

        public WebHostParams() {
            this.Port = "80";
            this.Name = "Default";
            this.Path = "";
            this.Protocol = "http";
        }

        [XmlAttribute]
        public String Name { get; set; }

        [XmlAttribute]
        public String Port { get; set; }

        [XmlAttribute]
        public String Path { get; set; }

        [XmlAttribute]
        public String Protocol { get; set; }

    }

    public class ServicesManager {

        private const string VNC_IP_SUBSTITUTION_STRING = "%%%IP%%%";
        private const string VNC_PORT_SUBSTITUTION_STRING = "%%%Port%%%";
        private const string VNC_USERNAME_SUBSTITUTION_STRING = "%%%UserName%%%";
        private const string VNC_PASSWORD_SUBSTITUTION_STRING = "%%%Password%%%";
        private const string VNC_CUSTOM1_SUBSTITUTION_STRING = "%%%Custom1%%%";

        [Serializable]
        [XmlRoot("Services")]
        public struct ServicesManagerConfig {

            public class VncServiceConfig {

                public VncServiceConfig() {
                    this.DefaultPassword = "";
                    this.DefaultUserName = "";
                    this.DefaultPort = "";
                    this.DefaultCustom1 = "";
                }

                public String Program { get; set; }

                /// <summary>
                /// Command Line to run the VNC Viewer Command Line.
                /// Following Substituions Allowed:
                ///     %%%IP%%% for the Current Hosts IP Address
                ///     %%%Port%%% for the Current Hosts Port Address
                ///     %%%UserName%%% for the Current Hosts UserName
                ///     %%%Password%%% for the Current Hosts Password
                ///     %%%Custom1%%% for a custom value
                /// </summary>
                public String Arguments { get; set; }

                public String DefaultPassword { get; set; }

                public String DefaultUserName { get; set; }

                public String DefaultPort { get; set; }

                public String DefaultCustom1 { get; set; }
            }

            public VncServiceConfig Vnc { get; set; }

        }

        private static ServicesManager singleton = null;

        public Boolean IsVncServiceAvailable { get; private set; }

        public Boolean IsShellServiceAvailable { get; private set; }

        public Boolean IsWebServiceAvailable { get; private set; }

        public static ServicesManager GetServicesManager() {
            if (singleton == null) {
                singleton = new ServicesManager();
            }
            return singleton;
        }

        private ServicesManagerConfig servicesManagerConfig;

        public ServicesManager() {
            if (ServicesManager.singleton != null) {
                throw new Exception("Trying to create another instance of ServicesManager.  This is a singleton class.  Come on Son!");
            }
            this.IsVncServiceAvailable = false;
            this.IsShellServiceAvailable = false;
            this.IsWebServiceAvailable = true;
            this.LoadServicesManagerConfigFile();
        }

        private void LoadServicesManagerConfigFile() {
            try {
                this.servicesManagerConfig = ConfigFileHelper.LoadConfigFile<ServicesManagerConfig>("Services.xml");
                this.IsVncServiceAvailable = true;
            } catch (ConfigFileDoesntExistException e) {
                System.Diagnostics.Debug.WriteLine("Services.xml doesn't exist, will be unable to run any external services.");
            }

        }

        public void LaunchVncServiceForHost(IPAddress ipAddress, VncHostParams vncHostParams) {
            String vncArguments = this.servicesManagerConfig.Vnc.Arguments;
            vncArguments = vncArguments.Replace(ServicesManager.VNC_IP_SUBSTITUTION_STRING, ipAddress.ToString());

            if (vncHostParams.Port != null) {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_PORT_SUBSTITUTION_STRING, vncHostParams.Port);
            } else {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_PORT_SUBSTITUTION_STRING, this.servicesManagerConfig.Vnc.DefaultPort);
            }

            if (vncHostParams.Password != null) {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_PASSWORD_SUBSTITUTION_STRING, vncHostParams.Password);
            } else {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_PASSWORD_SUBSTITUTION_STRING, this.servicesManagerConfig.Vnc.DefaultPassword);
            }

            if (vncHostParams.UserName != null) {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_USERNAME_SUBSTITUTION_STRING, vncHostParams.UserName);
            } else {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_USERNAME_SUBSTITUTION_STRING, this.servicesManagerConfig.Vnc.DefaultUserName);
            }

            if (vncHostParams.Custom1 != null) {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_CUSTOM1_SUBSTITUTION_STRING, vncHostParams.Custom1);
            } else {
                vncArguments = vncArguments.Replace(ServicesManager.VNC_CUSTOM1_SUBSTITUTION_STRING, this.servicesManagerConfig.Vnc.DefaultCustom1);
            }

            try {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = this.servicesManagerConfig.Vnc.Program;
                startInfo.Arguments = vncArguments;
                process.StartInfo = startInfo;
                process.Start();
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("Unable to launch VNC Service.");
            }
        }

        public void LaunchWebServiceForHost(IPAddress ipAddress, WebHostParams webHostParam) {
            System.Diagnostics.Process.Start(webHostParam.Protocol + "://" + ipAddress.ToString() + ":" + webHostParam.Port + "/" + webHostParam.Path);
        }

    }
}
