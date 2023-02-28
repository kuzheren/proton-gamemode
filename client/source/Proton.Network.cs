using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System;
using Proton;
using Proton.Stream;
using Proton.Packet.Serialization;
using Proton.Packet.ID;
using Proton.Packet.Handler;
using Proton.Global.States;
using Proton.Callbacks.Manager;

namespace Proton.Network
{
    public static class ProtonNetwork
    {
        public static TcpClient ProtonTCPClient;
        private static NetworkStream NetworkTCPStream;
        private static bool Active;
        private static byte[] receiveBuffer = new byte[65535];

        public static void Connect(string IP, int port)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ProtonCallbacksManager.InvokeCallback("OnClientError", new object[] {1, "Невозможно подключиться. Причина: отсутствие подключения к сети"});
                return;
            }

            try
            {
                ProtonTCPClient = new TcpClient(IP, port);
            }
            catch
            {
                ProtonCallbacksManager.InvokeCallback("OnClientError", new object[] {2, "Невозможно создать клиентский сокет. Причина: невозможность подключения к серверу"});
                return;
            }
            NetworkTCPStream = ProtonTCPClient.GetStream();

            ProtonGlobalStates.ConnectionState = ConnectionStates.AuthKeyRequest;
            Active = true;

            ProtonPacketSerializer authKeyRequest = new ProtonPacketSerializer(ProtonPacketID.AUTH_KEY_REQUEST);
            SendPacket(authKeyRequest);
        }
        public static void Disconnect()
        {
            ProtonGlobalStates.ConnectionState = ConnectionStates.Disconnected;
            Active = false;

            if (ProtonTCPClient != null)
            {
                NetworkTCPStream.Close();
                ProtonTCPClient.Close();
            }
        }
        public static void Receive()
        {
            if (Active == false)
            {
                return;
            }

            try
            {
                if (NetworkTCPStream.DataAvailable)
                {
                    byte[] packetLengthData = new byte[4];
                    NetworkTCPStream.Read(packetLengthData, 0, packetLengthData.Length);
                    
                    ProtonStream packetLengthPS = new ProtonStream();
                    packetLengthPS.Bytes = new List<byte>(packetLengthData);
                    uint packetLength = packetLengthPS.ReadUInt32();
                    List<byte> totalData = new List<byte>();

                    while((uint) totalData.Count < packetLength) // DANGEROUS!
                    {
                        byte[] additionBytesBuffer = new byte[65535];
                        int additionBytesAmmount = NetworkTCPStream.Read(additionBytesBuffer, 0, additionBytesBuffer.Length);
                        byte[] additionBytes = new byte[additionBytesAmmount];
                        Array.Copy(additionBytesBuffer, additionBytes, additionBytesAmmount);
                        totalData.AddRange(additionBytes);
                    }
                    ProtonStream RAWData = new ProtonStream();
                    RAWData.Bytes = totalData;
                    ProcessData(RAWData);
                }
            }
            catch (SocketException error)
            {
                switch (error.ErrorCode)
                {
                    case 10060:
                        return;
                    case 10054:
                        ProtonEngine.Disconnect();
                        break;
                }
            }
        }
        public static void Send(ProtonStream protonStream)
        {
            if (Active == false)
            {
                Debug.LogError("Вы должны подключиться для отправки данных!");
                return;
            }

            ProtonStream resultStream = new ProtonStream();
            resultStream.WriteUInt32((uint) protonStream.Bytes.Count);
            resultStream.WriteBytes(protonStream.Bytes.ToArray());

            try
            {
                NetworkTCPStream.Write(resultStream.Bytes.ToArray(), 0, resultStream.Bytes.ToArray().Length);
            }
            catch (SocketException error)
            {
                ProtonCallbacksManager.InvokeCallback("OnClientError", new object[] {4, "Произошла ошибка отправки. Причина: отключение сокета от сети"});
                ProtonEngine.Disconnect();
            }
        }
        public static void SendPacket(ProtonStream protonStream)
        {
            protonStream.Bytes.Insert(0, ProtonPacketID.PACKET);
            Send(protonStream);
        }
        public static void SendPacket(ProtonPacketSerializer protonPacketSerializer)
        {
            ProtonStream protonStream = protonPacketSerializer.protonStream;
            SendPacket(protonStream);
        }
        public static void SendPacket(ProtonPacketListSerializer protonPacketListSerializer)
        {
            ProtonStream protonStream = protonPacketListSerializer.protonStream;
            SendPacket(protonStream);
        }
        public static void SendRPC(ProtonStream protonStream)
        {
            protonStream.Bytes.Insert(0, ProtonPacketID.RPC);
            Send(protonStream);
        }
        public static void SendRPC(ProtonRPCSerializer protonRPCSerializer)
        {
            ProtonStream protonStream = protonRPCSerializer.protonStream;
            SendRPC(protonStream);
        }
        public static void ProcessData(ProtonStream data)
        {
            ProtonPacketHandler.ProcessData(data);
        }
        public static string GenerateAuthKeyResponse(string serverKey)
        {
            serverKey = serverKey.Substring(0, 16);
            using (var sha256 = SHA256.Create())
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(serverKey);
                byte[] hash = sha256.ComputeHash(keyBytes);
                string result = BitConverter.ToString(hash).Replace("-", "").Substring(0,16);
                return result;
            }
        }
    }
}