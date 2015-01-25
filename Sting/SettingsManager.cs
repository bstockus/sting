using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sting {
    public static class SettingsManager {

        [Serializable]
        public class Settings {

            public int BadPingStreakBeforeHostIsDown { get; set; }

        }

        private static Settings settings = null;

        public static Settings GetSettings() {
            if (settings == null) {
                settings = ConfigFileHelper.LoadConfigFile<Settings>("Settings.xml");
            }
            return settings;
        }

    }
}
