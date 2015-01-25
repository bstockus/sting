using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sting {

    public class ConfigFileDoesntExistException : Exception {

    }

    public static class ConfigFileHelper {

        public static T LoadConfigFile<T>(String fileName) {
            String configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.Combine("Sting", fileName));
            System.Diagnostics.Debug.WriteLine("ConfigFileHelper.LoadConfigFile<" + typeof(T).Name + ">(\"" + configFilePath + "\")");
            if (File.Exists(configFilePath)) {
                XmlSerializer deserializer = new XmlSerializer(typeof(T));
                TextReader reader = new StreamReader(configFilePath);
                T configData = (T)deserializer.Deserialize(reader);
                reader.Close();
                return configData;
            } else {
                throw new ConfigFileDoesntExistException();
            }
            
        }

    }
}
