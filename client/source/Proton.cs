using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proton.Network;
using Proton.Global.States;
using Proton.Structures;
using Proton.Packet.Handler;

namespace Proton
{
    public static class ProtonEngine
    {
        public static GameObject ProtonHandlerObject;
        public static Player LocalPlayer;
        public static Room CurrentRoom;
        private static string m_Nickname;
        public static string NickName
        {
            get
            {
                return m_Nickname;
            }
            set
            {
                m_Nickname = value;
                if (LocalPlayer != null)
                {
                    LocalPlayer.playerInfo.playerNickname = value;
                }
            }
        }

        public static void Connect(string IP, int port, string nickname)
        {
            if (IsConnected() == false)
            {
                GameObject ProtonHandlerPrefab = Resources.Load<GameObject>("ProtonHandler");
                ProtonHandlerObject = GameObject.Instantiate(ProtonHandlerPrefab);

                m_Nickname = nickname;
                ProtonNetwork.Connect(IP, port);
            }
            else
            {
                Debug.LogError("Вы уже подключены к серверу! Текущий ConnectionState: ConnectionStates." + ProtonGlobalStates.ConnectionState);
            }
        }
        public static void Disconnect()
        {
            GameObject.Destroy(ProtonHandlerObject);
            ProtonNetwork.Disconnect();

            LocalPlayer = null;
            CurrentRoom = null;
        }

        public static void JoinRoom(string roomName, string password="")
        {
            if (IsConnected() == false)
            {
                Debug.LogWarning("Невозможно выполнить операцию: вы отключены от сервера");
                return;
            }

            if (CurrentRoom != null)
            {
                Debug.LogWarning("Ошибка подключения к комнате! Вы уже находитесь в комнате!");
                return;
            }
            ProtonPacketHandler.SendJoinRoom(roomName, password);
        }
        public static void UpdateLocalPlayerInfo()
        {
            if (IsConnected() == false)
            {
                Debug.LogWarning("Невозможно выполнить операцию: вы отключены от сервера");
                return;
            }

            PlayerInfo updatedPlayerInfo = LocalPlayer.playerInfo;
            ProtonPacketHandler.SendUpdateLocalPlayerInfo(updatedPlayerInfo);
        }
        public static void UpdateRoomInfo()
        {
            if (CurrentRoom == null)
            {
                Debug.LogError("Вы не в комнате, чтобы обновлять её состояние на сервере!");
                return;
            }
            RoomInfo updatedRoomInfo = CurrentRoom.roomInfo;
            ProtonPacketHandler.UpdateRoomInfo(updatedRoomInfo);
        }
        public static void SendChatMessage(string chatMessage)
        {
            if (IsConnected() == false)
            {
                Debug.LogWarning("Невозможно выполнить операцию: вы отключены от сервера");
                return;
            }

            ProtonPacketHandler.SendChatMessage(chatMessage);
        }
        public static void SendRPC(params object[] values)
        {
            if (values.Length == 0)
            {
                Debug.LogError("Для отправки RPC нужно указать имя!");
                return;
            }
            else if (values.Length == 1)
            {
                Debug.LogError("Для отправки RPC нужно указать получателя!");
                return;
            }

            if (!IsConnected())
            {
                Debug.LogError("Для отправки RPC нужно подключиться к серверу!");
                return;
            }

            ProtonPacketHandler.SendRPC(values);
        }

        public static bool IsConnected()
        {
            return !(ProtonGlobalStates.ConnectionState == ConnectionStates.Disconnected || ProtonGlobalStates.ConnectionState == ConnectionStates.AuthKeyRequest);
        }
    }
    public class Room
    {
        public RoomInfo roomInfo;
        public List<Player> playersList = new List<Player>();

        public Room() {}
        public Room(RoomInfo roomInfo)
        {
            this.roomInfo = roomInfo;
        }

        public override string ToString()
        {
            return $"Room[{this.roomInfo.roomName}], map[{this.roomInfo.mapName}], online[{this.roomInfo.currentPlayers}/{this.roomInfo.maxPlayers}]";
        }

        public void AddOrUpdatePlayer(Player player)
        {
            foreach (Player existedPlayer in playersList)
            {
                if (existedPlayer == player)
                {
                    playersList.Remove(existedPlayer);
                    playersList.Add(player);
                    return;
                }
            }
            playersList.Add(player);
        }
        public void RemovePlayer(Player removedPlayer)
        {
            foreach (Player existedPlayer in playersList)
            {
                if (existedPlayer == removedPlayer)
                {
                    playersList.Remove(existedPlayer);
                    return;
                }
            }
        }
        public Player FindPlayerByID(uint ID)
        {
            foreach (Player player in playersList)
            {
                if (player.playerInfo.ID == ID)
                {
                    return player;
                }
            }
            return null;
        }
        public Player FindPlayerByNickName(string nickname)
        {
            foreach (Player player in playersList)
            {
                if (player.playerInfo.playerNickname == nickname)
                {
                    return player;
                }
            }
            return null;
        }
        public void UpdatePlayersInfo(List<Player> newInfo)
        {
            foreach (Player existedPlayer in playersList)
            {
                foreach (Player newPlayerObject in newInfo)
                {
                    if (existedPlayer == newPlayerObject)
                    {
                        existedPlayer.playerInfo = newPlayerObject.playerInfo;
                    }
                }
            }
        }
    }
    public class Player
    {
        public PlayerInfo playerInfo;

        public Player() {}
        public Player(PlayerInfo playerInfo)
        {
            this.playerInfo = playerInfo;
        }

        public override string ToString()
        {
            return $"[{this.playerInfo.ID}]({this.playerInfo.playerNickname}), ping: ({this.playerInfo.ping.ToString()})";
        }
        public override bool Equals(object secondPlayer)
        {
            if (secondPlayer is null)
            {
                return false;
            }
            return (this.playerInfo.ID == ((Player) secondPlayer).playerInfo.ID);
        }
        public static bool operator ==(Player firstPlayer, Player secondPlayer)
        {
            if (firstPlayer is null || secondPlayer is null)
            {
                return false;
            }
            return (firstPlayer.playerInfo.ID == secondPlayer.playerInfo.ID);
        }
        public static bool operator !=(Player firstPlayer, Player secondPlayer)
        {
            if (firstPlayer is null || secondPlayer is null)
            {
                return true;
            }
            return (firstPlayer.playerInfo.ID != secondPlayer.playerInfo.ID);
        }
        public override int GetHashCode()
        {
            return (int)(this.playerInfo.ID / (uint) 2);
        }
    }
}