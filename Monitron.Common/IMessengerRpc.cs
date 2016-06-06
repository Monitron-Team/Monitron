using System;

namespace Monitron.Common
{
    public interface IMessengerRpc
    {
        T CreateRpcClient<T>(Identity target) where T : class;
        void RegisterRpcServer<I, T>(T server)
            where I : class
            where T : I;
    }
}

