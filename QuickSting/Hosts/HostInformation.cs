using System;
using System.Xml.Serialization;
namespace QuickSting {

    [Serializable]
    public struct HostInformation {

        [XmlAttribute("HostOctet")]
        public string HostOctet { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlElement("Service")]
        public Service[] Services { get; set; }

    }

    public struct HostDefinition {

        public string HostOctet { get; set; }

        public string Name { get; set; }

        public string GroupName { get; set; }

        public string Description { get; set; }

        public Service[] Services { get; set; }

    }

}
