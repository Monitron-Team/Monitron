using System.Runtime.Serialization.Json;

namespace Monitron.Common
{
    public interface IPluginDataStore
    {
        void Write<T>(string i_Key, T i_Value);
        T Read<T>(string i_Key);
        void Delete(string i_Key);
    }
}
