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
