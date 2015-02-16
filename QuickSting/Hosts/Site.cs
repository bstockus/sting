using System;
using System.Xml.Serialization;

namespace QuickSting {

    [Serializable]
    public class Site {

        [XmlElement("PicturesFolderUrl")]
        public string PicturesFolderUrl { get; set; }

        [XmlElement("PhoneNumberUrl")]
        public string PhoneNumberUrl { get; set; }

        [XmlElement("EmailUrl")]
        public string EmailUrl { get; set; }

    }
}
