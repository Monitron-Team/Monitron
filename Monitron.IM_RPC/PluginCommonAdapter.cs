using System;

using Monitron.Common;

namespace Monitron.ImRpc
{
    public class PluginCommonAdapter
    {
        object m_Obj;
        IMessengerClient m_MessangerClient;
        public PluginCommonAdapter(object i_Obj, IMessengerClient i_MessangerClient)
        {
            m_Obj = i_Obj;
            i_MessangerClient.MessageArrived += DoWhenMessageAvrrived;
            m_MessangerClient = i_MessangerClient;
        }
        public void DoWhenMessageAvrrived(object i_Sender, MessageArrivedEventArgs i_EventArgs)
        {
            Command cmd = Command.Parse(i_EventArgs.Message);
            string retuenedValue;
            bool wasSuccess = ImRpcCache.ParseExecute(m_Obj, cmd, out retuenedValue);
            if (wasSuccess)
            {
                Console.WriteLine(retuenedValue);  //TODO: response to instant message. how??
            }
            else
            {
                retuenedValue = string.Format("Error executing: \"{0}\"", i_EventArgs.Message);
            }
            try
            {
                m_MessangerClient.sendMessage(i_EventArgs.Buddy, retuenedValue);
            }
            catch (Exception)
            {
                //todo
            }
        }
    }
}
