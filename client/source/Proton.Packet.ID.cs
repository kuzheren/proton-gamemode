using System.Collections;
using System.Collections.Generic;

namespace Proton.Packet.ID
{
    public static class ProtonPacketID
    {
        public static readonly byte PACKET =                                   30;
        public static readonly byte RPC =                                      31;
        public static readonly byte PING =                                     32;
        public static readonly byte PONG =                                     33;

        public static readonly byte AUTH_KEY_REQUEST =                         0;
        public static readonly byte AUTH_KEY =                                 1;
        public static readonly byte CONNECTION_REQUEST =                       2;
        public static readonly byte CONNECTION_REQUEST_ACCEPTED =              3;
        public static readonly byte JOIN_ROOM_REQUEST_ACCEPTED =               5;
        public static readonly byte PLAYER_CLASS_INFO =                        6;
        public static readonly byte JOIN_ROOM_BY_NAME_REQUEST =                9;
        public static readonly byte REMOVE_PLAYER_CLASS_INFO =                 13;
        public static readonly byte UPDATE_ROOM_INFO =                         15;
        public static readonly byte CHAT_MESSAGE =                             20;
        public static readonly byte SERVER_ERROR =                             21;
        public static readonly byte KICK =                                     22;

        public static readonly byte BOOL =                                     0;
        public static readonly byte BYTE =                                     1;
        public static readonly byte UINT16 =                                   2;
        public static readonly byte INT16 =                                    3;
        public static readonly byte UINT32 =                                   4;
        public static readonly byte INT32 =                                    5;
        public static readonly byte FLOAT =                                    6;
        public static readonly byte STRING =                                   7;
        public static readonly byte VECTOR3 =                                  8;
        public static readonly byte QUATERNION =                               9;
        public static readonly byte ARRAY =                                    10;
        public static readonly byte DICTIONARY =                               11;
        public static readonly byte BYTEARRAY =                                12;
    }
    public static class RPCTarget
    {
        public static readonly uint SERVER =                                   0;
        public static readonly uint ROOM =                                     1;
        public static readonly uint GLOBAL =                                   3;
    }
}