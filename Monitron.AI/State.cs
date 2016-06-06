using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIMLbot.Utils;

namespace Monitron.AI
{
    public class State
    {
        private SettingsDictionary UserSettings;

        public State(SettingsDictionary i_UserSettings)
        {
            UserSettings = i_UserSettings;
        }

        public string getKey(string i_Key)
        {
            string result = null;
            if (this.UserSettings.containsSettingCalled(i_Key))
            {
                result =  UserSettings.grabSetting(i_Key);
            }
            return result;
        }

        public string[] GetAllKeys()
        {
            return this.UserSettings.SettingNames;
        }
    }
}
