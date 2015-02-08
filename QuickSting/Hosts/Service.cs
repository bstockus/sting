using System;
using System.Net;
using System.Xml.Serialization;

namespace QuickSting {

    [Serializable]
    public struct Service {

        [XmlAttribute("Type")]
        public string ServiceType { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Program")]
        public string Program { get; set; }

        [XmlText]
        public string CommandLine { get; set; }

        public void Launch(IPAddress ipAddress) {
            string commandLine = this.CommandLine;
            if (commandLine == null) commandLine = "";
            commandLine = commandLine.Replace("%%%IP%%%", ipAddress.ToString());
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.Arguments = commandLine;
            startInfo.FileName = this.Program.Replace("%%%IP%%%", ipAddress.ToString()); ;
            System.Diagnostics.Process.Start(startInfo);
        }

    }

}
