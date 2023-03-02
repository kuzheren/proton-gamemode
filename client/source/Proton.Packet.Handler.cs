using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Proton.Stream;
using Proton.Packet.ID;
using Proton.Global.States;
using Proton.Packet.Serialization;
using Proton.Network;
using Proton.Structures;
using Proton;
using Proton.Callbacks.Manager;
using System;
using System.Linq;
using System.Reflection;

namespace Proton.Packet.Handler
{
    public static class ProtonPacketHandler
    {
        public static List<Player> cachedPlayers = new List<Player>();

        public static void ProcessData(ProtonStream data)
        {
            byte dataID = data.Read<byte>();
            
            if (dataID == ProtonPacketID.PACKET)
            {
                NetworkDictionary customPacketData = data.ReadNetworkDictionary();
                ProcessPacket(data, customPacketData);
            }
            else if (dataID == ProtonPacketID.RPC)
            {
                ProcessRPC(data);
            }
            else if (dataID == ProtonPacketID.PING)
            {
                SendPongPacket();
            }
        }
        public static void ProcessPacket(ProtonStream packet, NetworkDictionary customPacketData)
        {
            byte packetID = packet.Read<byte>();

            //if (packetID != (byte) 6)
            //{
            //    Debug.Log($"New packet: {packetID}");
            //}

            if (packetID == ProtonPacketID.AUTH_KEY)
            {
                if (ProtonGlobalStates.ConnectionState != ConnectionStates.AuthKeyRequest)
                {
                    return;
                }

                ProtonGlobalStates.ConnectionState = ConnectionStates.ConnectionRequest;

                ProtonPacketDeserializer authKeyData = new ProtonPacketDeserializer(packet, typeof(AuthKey));
                AuthKey authKeyStructure = (AuthKey) authKeyData.structure;

                ConnectionRequest connectionRequestStructure = new ConnectionRequest(ProtonEngine.NickName, Application.version, ProtonNetwork.GenerateAuthKeyResponse(authKeyStructure.authKey));
                ProtonPacketSerializer connectionRequest = new ProtonPacketSerializer(ProtonPacketID.CONNECTION_REQUEST, connectionRequestStructure);
                ProtonNetwork.SendPacket(connectionRequest);
            }
            else if (packetID == ProtonPacketID.CONNECTION_REQUEST_ACCEPTED)
            {
                ProtonGlobalStates.ConnectionState  = ConnectionStates.Connected;

                ProtonPacketDeserializer connectionRequestAcceptedData = new ProtonPacketDeserializer(packet, typeof(ConnectionRequestAccepted));
                ConnectionRequestAccepted connectionRequestAcceptedStructure = (ConnectionRequestAccepted) connectionRequestAcceptedData.structure;

                ProtonCallbacksManager.InvokeCallback("OnConnected", new object[] {connectionRequestAcceptedStructure.serverVersion, connectionRequestAcceptedStructure.gameVersion, connectionRequestAcceptedStructure.serverName});
                ProtonGlobalStates.LastPingTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            else if (packetID == ProtonPacketID.PLAYER_CLASS_INFO)
            {
                bool isList = (bool) customPacketData["isList"].value;

                if (isList == true)
                {
                    ProtonPacketListDeserializer playersListData = new ProtonPacketListDeserializer(packet, typeof(PlayerInfo));
                    List<Player> playersList = new List<Player>();
                    foreach (object playerInfo in playersListData.structures)
                    {
                        playersList.Add(new Player((PlayerInfo) playerInfo));
                    }
                    cachedPlayers = playersList;

                    bool isUpdate = (bool) customPacketData["update"].value;
                    if (isUpdate)
                    {
                        if (ProtonEngine.CurrentRoom == null)
                        {
                            return;
                        }
                        ProtonEngine.CurrentRoom.UpdatePlayersInfo(cachedPlayers);
                        cachedPlayers = new List<Player>();
                        ProtonCallbacksManager.InvokeCallback("OnPlayersInfoUpdated", new object[] {playersList});
                    }
                }
                else
                {
                    ProtonPacketDeserializer playerData = new ProtonPacketDeserializer(packet, typeof(PlayerInfo));
                    PlayerInfo playerInfo = (PlayerInfo) playerData.structure;
                    Player player = new Player(playerInfo);
                    bool isLocal = (bool) customPacketData["local"].value;

                    if (isLocal)
                    {
                        ProtonEngine.LocalPlayer = player;
                    }
                    else
                    {
                        ProtonEngine.CurrentRoom.AddOrUpdatePlayer(player);
                        ProtonCallbacksManager.InvokeCallback("OnPlayerJoined", new object[] {player});
                    }
                }
            }
            else if (packetID == ProtonPacketID.JOIN_ROOM_REQUEST_ACCEPTED)
            {
                ProtonPacketDeserializer joinedRoomInfoData = new ProtonPacketDeserializer(packet, typeof(RoomInfo));
                RoomInfo joinedRoomInfo = (RoomInfo) joinedRoomInfoData.structure;
                Room joinedRoom = new Room(joinedRoomInfo);
                joinedRoom.playersList = cachedPlayers;
                cachedPlayers = new List<Player>();
                
                ProtonEngine.CurrentRoom = joinedRoom;
                ProtonGlobalStates.ConnectionState = ConnectionStates.JoinedToRoom;
                ProtonCallbacksManager.InvokeCallback("OnJoinRoom", new object[] {joinedRoom});
            }
            else if (packetID == ProtonPacketID.REMOVE_PLAYER_CLASS_INFO)
            {
                ProtonPacketDeserializer quitedPlayerDeserializer = new ProtonPacketDeserializer(packet, typeof(RemovedPlayerInfo));
                RemovedPlayerInfo removedPlayerInfo = (RemovedPlayerInfo) quitedPlayerDeserializer.structure;
                uint removedPlayerID = removedPlayerInfo.ID;
                Player quitedPlayer = ProtonEngine.CurrentRoom.FindPlayerByID(removedPlayerID);
                if (quitedPlayer == null)
                {
                    return;
                }
                ProtonCallbacksManager.InvokeCallback("OnPlayerLeaved", new object[] {quitedPlayer});
                ProtonEngine.CurrentRoom.RemovePlayer(quitedPlayer);
            }
            else if (packetID == ProtonPacketID.UPDATE_ROOM_INFO)
            {
                if (ProtonEngine.CurrentRoom == null)
                {
                    return;
                }
                ProtonPacketDeserializer updatedInfoSerializer = new ProtonPacketDeserializer(packet, typeof(RoomInfo));
                RoomInfo updatedInfo = (RoomInfo) updatedInfoSerializer.structure;
                ProtonEngine.CurrentRoom.roomInfo = updatedInfo;
                ProtonCallbacksManager.InvokeCallback("OnRoomInfoUpdated", new object[] {updatedInfo});
            }
            else if (packetID == ProtonPacketID.CHAT_MESSAGE)
            {
                ProtonPacketDeserializer chatMessageDeserializer = new ProtonPacketDeserializer(packet, typeof(ChatMessage));
                string chatMessage = ((ChatMessage) chatMessageDeserializer.structure).chatMessage;
                ProtonCallbacksManager.InvokeCallback("OnChatMessage", new object[] {chatMessage});
            }
            else if (packetID == ProtonPacketID.SERVER_ERROR)
            {
                ProtonPacketDeserializer serverErrorDeserializer = new ProtonPacketDeserializer(packet, typeof(ServerError));
                int errorCode = ((ServerError) serverErrorDeserializer.structure).errorCode;
                string errorMessage = ((ServerError) serverErrorDeserializer.structure).errorMessage;
                ProtonCallbacksManager.InvokeCallback("OnServerError", new object[] {errorCode, errorMessage});
            }
            else if (packetID == ProtonPacketID.KICK)
            {
                ProtonPacketDeserializer kickNotificationDeserializer = new ProtonPacketDeserializer(packet, typeof(KickNotification));
                string kickReason = ((KickNotification) kickNotificationDeserializer.structure).reason;
                ProtonCallbacksManager.InvokeCallback("OnKicked", new object[] {kickReason});
                ProtonEngine.Disconnect();
            }
        }

        public static void ProcessRPC(ProtonStream protonStream)
        {
            ProtonRPCDeserializer RPCDeserializer = new ProtonRPCDeserializer(protonStream);
            List<NetworkValue> networkValues = RPCDeserializer.networkValues;
            List<object> networkObjects = new List<object>();
            foreach (NetworkValue value in networkValues)
            {
                networkObjects.Add(value.value);
            }

            ProtonCallbacksManager.InvokeCallback(RPCDeserializer.RPCName, networkObjects.ToArray());
        }

        public static void SendRPC(object[] values)
        {
            string RPCName = (string) values[0];
            uint targetID = 0;

            if (values[1].GetType() == typeof(uint))
            {
                targetID = (uint) values[1];
            }
            else if (values[1].GetType() == typeof(Player))
            {
                targetID = ((Player) values[1]).playerInfo.ID;
                values[1] = targetID;
            }
            else
            {
                Debug.LogError("Для отправки RPC нужно выбрать ID или класс отправителя!");
                return;
            }

            ProtonRPCSerializer RPCSerializer = new ProtonRPCSerializer(values);
            ProtonNetwork.SendRPC(RPCSerializer.protonStream);
        }

        public static void SendPongPacket()
        {
            ProtonGlobalStates.LastPingTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            ProtonStream pongProtonStream = new ProtonStream();
            pongProtonStream.WriteByte(ProtonPacketID.PONG);
            ProtonNetwork.Send(pongProtonStream);
        }
        public static void SendJoinRoom(string roomName, string password)
        {
            ProtonPacketSerializer joinRoomSerializer = new ProtonPacketSerializer(ProtonPacketID.JOIN_ROOM_BY_NAME_REQUEST, new JoinRoomRequest(roomName, password));
            ProtonNetwork.SendPacket(joinRoomSerializer);
        }
        public static void SendUpdateLocalPlayerInfo(PlayerInfo updatedPlayerInfo)
        {
            ProtonPacketSerializer updatedInfoSerializer = new ProtonPacketSerializer(ProtonPacketID.PLAYER_CLASS_INFO, updatedPlayerInfo);
            ProtonNetwork.SendPacket(updatedInfoSerializer);
        }
        public static void UpdateRoomInfo(RoomInfo updatedRoomInfo)
        {
            ProtonPacketSerializer updatedInfoSerializer = new ProtonPacketSerializer(ProtonPacketID.UPDATE_ROOM_INFO, updatedRoomInfo);
            ProtonNetwork.SendPacket(updatedInfoSerializer); 
        }
        public static void SendChatMessage(string chatMessage)
        {
            ChatMessage chatMessageInfo = new ChatMessage(chatMessage);
            ProtonPacketSerializer chatMessageSerializer = new ProtonPacketSerializer(ProtonPacketID.CHAT_MESSAGE, chatMessageInfo);
            ProtonNetwork.SendPacket(chatMessageSerializer);
        }
    }
}