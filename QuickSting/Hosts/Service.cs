using System;
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

    }

}
