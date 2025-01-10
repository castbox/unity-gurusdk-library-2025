namespace Guru.Network
{
    using System;
    public interface IConnectivityWatcher
    {
        void SetNetworkStatusListener(Action<string[]> handler);
        string[] GetNetConnectivity();
    }
}