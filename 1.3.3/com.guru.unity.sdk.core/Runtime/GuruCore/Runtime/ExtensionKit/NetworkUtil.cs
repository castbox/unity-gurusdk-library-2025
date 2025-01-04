using UnityEngine;

public static class NetworkUtil
{
    public static bool IsNetAvailable => Application.internetReachability != NetworkReachability.NotReachable;
}