using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIMLbot;


namespace Monitron.AI
{
    public class AIML
    {
        private Bot m_bot;

        private User m_User;

        public AIML()
        {
            m_bot = new Bot();
            m_bot.loadSettings();
            m_User = new User("consoleUser", m_bot);
            m_bot.isAcceptingUserInput = false;
            m_bot.loadAIMLFromFiles();
            m_bot.isAcceptingUserInput = true;
        }

        public string Request(string i_message, out bool o_Is_Mothes)
        {
            Request r = new Request(i_message, this.m_User, this.m_bot);
            Result res = this.m_bot.Chat(r);
            bool is_method = r.bot.GlobalSettings.containsSettingCalled("is_method")  && 
                            r.bot.GlobalSettings.grabSetting("is_method") == "True";
            if (is_method)
            {
                o_Is_Mothes = true;
            }
            else
            {
                o_Is_Mothes = false;
            }
            return res.Output;


        }
    }
}
