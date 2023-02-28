using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proton.Global.States
{
    public enum ConnectionStates
    {
        Disconnected,
        AuthKeyRequest,
        ConnectionRequest,
        Connected,
        JoiningToRoom,
        JoinedToRoom
    }

    public static class ProtonGlobalStates
    {
        public static ConnectionStates ConnectionState = ConnectionStates.Disconnected;
        public static long LastPingTime;
    }
}