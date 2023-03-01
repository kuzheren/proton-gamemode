using System.Collections;
using System.Collections.Generic;
using Proton.Packet.Serialization;
using UnityEngine;

namespace Proton.Structures
{
    public class ConnectionRequest
    {
        public string nickname;
        public string gameVersion;
        public string authKeyResponse;

        public ConnectionRequest() {}
        public ConnectionRequest(string nickname, string gameVersion, string authKeyResponse)
        {
            this.nickname = nickname;
            this.gameVersion = gameVersion;
            this.authKeyResponse = authKeyResponse;
        }
    }
    public class ConnectionRequestAccepted
    {
        public string serverVersion;
        public string gameVersion;
        public string serverName;

        public ConnectionRequestAccepted() {}
        public ConnectionRequestAccepted(string serverVersion, string gameVersion, string serverName)
        {
            this.serverVersion = serverVersion;
            this.gameVersion = gameVersion;
            this.serverName = serverName;
        }
    }
    public class AuthKey
    {
        public string authKey;
    }
    public class RoomInfo
    {
        public string roomName;
        public string mapName;
        public ushort currentPlayers;
        public ushort maxPlayers;
        public string password;
        public NetworkDictionary customRoomParameters;
        public bool isOpen = true;
        public bool isVisible = true;
        public string roomJoinCode;

        public RoomInfo() {}
        public RoomInfo(string roomName, string mapName, ushort maxPlayers, string password, NetworkDictionary customRoomParameters=null, bool isOpen=true, bool isVisible=true, string roomJoinCode="")
        {
            if (customRoomParameters == null)
            {
                customRoomParameters = new NetworkDictionary();
            }
            this.roomName = roomName;
            this.mapName = mapName;
            this.maxPlayers = maxPlayers;
            this.password = password;
            this.customRoomParameters = customRoomParameters;
            this.isOpen = isOpen;
            this.isVisible = isVisible;
            this.roomJoinCode = roomJoinCode;
        }
    }
    public class PlayerInfo
    {
        public uint ID;
        public string playerNickname;
        public float ping;
        public NetworkDictionary customPlayerParameters;

        public PlayerInfo() {}
        public PlayerInfo(uint ID, string playerNickname, float ping, NetworkDictionary customPlayerParameters)
        {
            this.ID = ID;
            this.playerNickname = playerNickname;
            this.ping = ping;
            this.customPlayerParameters = customPlayerParameters;
        }
    }
    public class JoinRoomRequest
    {
        public string roomName;
        public string password;

        public JoinRoomRequest(string roomName, string password)
        {
            this.roomName = roomName;
            this.password = password;
        }
    }
    public class JoinRoomByCodeRequest
    {
        public string roomCode;

        public JoinRoomByCodeRequest(string roomCode)
        {
            this.roomCode = roomCode;
        }
    }
    public class RemovedPlayerInfo
    {
        public uint ID;

        public RemovedPlayerInfo() {}
        public RemovedPlayerInfo(uint ID)
        {
            this.ID = ID;
        }
    }
    public class ChangedHostInfo
    {
        public uint ID;

        public ChangedHostInfo() {}
        public ChangedHostInfo(uint ID)
        {
            this.ID = ID;
        }
    }
    public class ChatMessage
    {
        public string chatMessage;
        
        public ChatMessage() {}
        public ChatMessage(string chatMessage)
        {
            this.chatMessage = chatMessage;
        }
    }
    public class ServerError
    {
        public int errorCode;
        public string errorMessage;

        public ServerError() {}
        public ServerError(int errorCode, string errorMessage)
        {
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
        }
    }
    public class KickNotification
    {
        public string reason;

        public KickNotification() {}
        public KickNotification(string reason)
        {
            this.reason = reason;
        }
    }
}