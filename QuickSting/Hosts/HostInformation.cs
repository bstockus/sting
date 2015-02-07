using System;
using System.Xml.Serialization;
namespace QuickSting {

    [Serializable]
    public struct HostInformation {

        [XmlAttribute("HostOctet")]
        public string HostOctet { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Group")]
        public string GroupName { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlElement("Service")]
        public Service[] Services { get; set; }

    }

}
