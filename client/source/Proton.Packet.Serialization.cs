using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Proton.Stream;
using Proton.Packet.ID;

namespace Proton.Packet.Serialization
{
    public class ProtonPacketSerializer
    {
        public ProtonStream protonStream;

        public ProtonPacketSerializer()
        {
            throw new ArgumentException("ProtonPacketSerializer require arguments.");
        }
        public ProtonPacketSerializer(byte packetID)
        {
            protonStream = new ProtonStream();
            protonStream.WriteByte(packetID);
        }
        public ProtonPacketSerializer(byte packetID, object structure)
        {
            protonStream = ConvertStructureToProtonStream(packetID, structure);
        }

        public ProtonStream ConvertStructureToProtonStream(byte packetID, object targetStructure)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            ProtonStream resultProtonStream = new ProtonStream();
            resultProtonStream.WriteByte(packetID);

            foreach (FieldInfo field in targetStructure.GetType().GetFields(bindingFlags))
            {
                if (field.FieldType == typeof(string))
                {
                    resultProtonStream.WriteString((string) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(byte))
                {
                    resultProtonStream.WriteByte((byte) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(float))
                {
                    resultProtonStream.WriteFloat((float) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(bool))
                {
                    resultProtonStream.WriteBool((bool) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(uint))
                {
                    resultProtonStream.WriteUInt32((uint) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(int))
                {
                    resultProtonStream.WriteInt32((int) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(ushort))
                {
                    resultProtonStream.WriteUInt16((ushort) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(short))
                {
                    resultProtonStream.WriteInt16((short) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    resultProtonStream.WriteVector3((Vector3) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(Quaternion))
                {
                    resultProtonStream.WriteQuaternion((Quaternion) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(NetworkDictionary))
                {
                    resultProtonStream.WriteNetworkDictionary((NetworkDictionary) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(byte[]))
                {
                    resultProtonStream.WriteBytearray((byte[]) field.GetValue(targetStructure));
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    resultProtonStream.WriteVector2((Vector2) field.GetValue(targetStructure));
                }
            }
            return resultProtonStream;
        }
    }

    public class ProtonPacketDeserializer
    {
        public object structure;

        public ProtonPacketDeserializer(ProtonStream packet, Type targetStructure)
        {
            structure = ConvertProtonStreamToStruct(packet, targetStructure);
        }

        private object ConvertProtonStreamToStruct(ProtonStream packet, Type targetStructPattern)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            object newClass = System.Activator.CreateInstance(targetStructPattern);
            foreach (FieldInfo field in targetStructPattern.GetFields(bindingFlags))
            {
                string fieldName = field.Name;
                FieldInfo targetField = newClass.GetType().GetField(fieldName);

                if (field.FieldType == typeof(string))
                {
                    targetField.SetValue(newClass, packet.Read<string>());
                }
                else if (field.FieldType == typeof(byte))
                {
                    targetField.SetValue(newClass, packet.Read<byte>());
                }
                else if (field.FieldType == typeof(float))
                {
                    targetField.SetValue(newClass, packet.Read<float>());
                }
                else if (field.FieldType == typeof(bool))
                {
                    targetField.SetValue(newClass, packet.Read<bool>());
                }
                else if (field.FieldType == typeof(uint))
                {
                    targetField.SetValue(newClass, packet.Read<uint>());
                }
                else if (field.FieldType == typeof(int))
                {
                    targetField.SetValue(newClass, packet.Read<int>());
                }
                else if (field.FieldType == typeof(ushort))
                {
                    targetField.SetValue(newClass, packet.Read<ushort>());
                }
                else if (field.FieldType == typeof(short))
                {
                    targetField.SetValue(newClass, packet.Read<short>());
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    targetField.SetValue(newClass, packet.Read<Vector3>());
                }
                else if (field.FieldType == typeof(Quaternion))
                {
                    targetField.SetValue(newClass, packet.Read<Quaternion>());
                }
                else if (field.FieldType == typeof(NetworkDictionary))
                {
                    targetField.SetValue(newClass, packet.ReadNetworkDictionary());
                }
                else if (field.FieldType == typeof(byte[]))
                {
                    targetField.SetValue(newClass, packet.ReadBytearray());
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    targetField.SetValue(newClass, packet.ReadVector2());
                }
            }
            return newClass;
        }
    }

    public class ProtonRPCSerializer
    {
        public ProtonStream protonStream;

        public ProtonRPCSerializer(object[] values)
        {
            protonStream = SerializeRPCValuesToProtonStream(values);
        }
        public ProtonStream SerializeRPCValuesToProtonStream(object[] values)
        {
            ProtonStream serializedProtonStream = new ProtonStream();
            ushort argumentsCount = (ushort) (values.Length - 2);
            string RPCName = (string) values[0];
            uint targetID = (uint) values[1];

            serializedProtonStream.WriteUInt32(targetID);
            serializedProtonStream.WriteString(RPCName);
            serializedProtonStream.WriteUInt16(argumentsCount);

            for (int i = 2; i < values.Length; i++)
            {
                object value = values[i];

                if (value == null)
                {
                    Debug.LogError("Предотвращена попытка отправки RPC с null аргументом! Функция: " + RPCName + ". Индекс аргумента: " + (i - 1));
                    return null;
                }

                byte typeID = ProtonTypes.GetTypeID(value.GetType());
                NetworkValue resultNetworkValue = new NetworkValue(typeID, System.Convert.ChangeType(value, value.GetType()));
                serializedProtonStream.WriteNetworkValue(resultNetworkValue);
            }

            return serializedProtonStream;
        }
    }

    public class ProtonRPCDeserializer
    {
        public string RPCName;
        public uint senderID;
        public List<NetworkValue> networkValues;

        public ProtonRPCDeserializer(ProtonStream packet)
        {
            networkValues = DeserializeRPC(packet);
        }

        public List<NetworkValue> DeserializeRPC(ProtonStream packet)
        {
            List<NetworkValue> result = new List<NetworkValue>();

            senderID = packet.ReadUInt32();
            RPCName = packet.ReadString();
            ushort argumentsCount = packet.ReadUInt16();
            for (int i = 0; i < (int) argumentsCount; i++)
            {
                result.Add(packet.ReadNetworkValue());
            }

            return result;
        }
    }

    public class ProtonPacketListSerializer
    {
        public ProtonStream protonStream;
        public List<object> structures;

        public ProtonPacketListSerializer(byte packetID, List<object> structures)
        {
            protonStream = new ProtonStream();
            protonStream.WriteByte(packetID);
            protonStream.WriteUInt16((ushort) structures.Count);

            foreach (object structure in structures)
            {
                ProtonPacketSerializer serializer = new ProtonPacketSerializer(packetID, structure);
                serializer.protonStream.Bytes.RemoveAt(0);
                protonStream.WriteBytes(serializer.protonStream.Bytes.ToArray());
            }
        }
    }

    public class ProtonPacketListDeserializer
    {
        public List<object> structures;

        public ProtonPacketListDeserializer(ProtonStream packet, Type targetStruct)
        {
            ushort listSize = packet.Read<ushort>();
            structures = new List<object>();

            for (int i = 0; i < listSize; i++)
            {
                ProtonPacketDeserializer deserializer = new ProtonPacketDeserializer(packet, targetStruct);
                structures.Add(deserializer.structure);
            }
        }
    }
    
    public class NetworkValue
    {
        public byte type;
        public object value;

        public NetworkValue(byte type, object value)
        {
            this.type = type;
            this.value = value;
        }
    }

    public class NetworkDictionary
    {
        public Dictionary<string, NetworkValue> dictionary;

        public NetworkDictionary()
        {
            dictionary = new Dictionary<string, NetworkValue>();
        }

        public NetworkValue this[string key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                dictionary[key] = (NetworkValue) value;
            }
        }

        public void Remove(string key)
        {
            dictionary.Remove(key);
        }
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }
        public bool ContainsValue(NetworkValue value)
        {
            return dictionary.ContainsValue(value);
        }
        public void Clear()
        {
            dictionary.Clear();
        }
    }

    public static class ProtonTypes
    {
        public static Type GetTypeByID(byte typeID)
        {
            if (typeID == ProtonPacketID.BYTE)
            {
                return typeof(byte);
            }
            else if (typeID == ProtonPacketID.STRING)
            {
                return typeof(string);
            }
            else if (typeID == ProtonPacketID.UINT16)
            {
                return typeof(ushort);
            }
            else if (typeID == ProtonPacketID.INT16)
            {
                return typeof(short);
            }
            else if (typeID == ProtonPacketID.UINT32)
            {
                return typeof(uint);
            }
            else if (typeID == ProtonPacketID.INT32)
            {
                return typeof(int);
            }
            else if (typeID == ProtonPacketID.FLOAT)
            {
                return typeof(float);
            }
            else if (typeID == ProtonPacketID.BOOL)
            {
                return typeof(bool);
            }
            else if (typeID == ProtonPacketID.VECTOR3)
            {
                return typeof(Vector3);
            }
            else if (typeID == ProtonPacketID.QUATERNION)
            {
                return typeof(Quaternion);
            }
            else if (typeID == ProtonPacketID.DICTIONARY)
            {
                return typeof(NetworkDictionary);
            }
            else if (typeID == ProtonPacketID.BYTEARRAY)
            {
                return typeof(byte[]);
            }
            else if (typeID == ProtonPacketID.VECTOR2)
            {
                return typeof(Vector2);
            }
            return null;
        }
        public static byte GetTypeID(Type type)
        {
            if (type == typeof(byte))
            {
                return ProtonPacketID.BYTE;
            }
            else if (type == typeof(string))
            {
                return ProtonPacketID.STRING;
            }
            else if (type == typeof(ushort))
            {
                return ProtonPacketID.UINT16;
            }
            else if (type == typeof(short))
            {
                return ProtonPacketID.INT16;
            }
            else if (type == typeof(uint))
            {
                return ProtonPacketID.UINT32;
            }
            else if (type == typeof(int))
            {
                return ProtonPacketID.INT32;
            }
            else if (type == typeof(float))
            {
                return ProtonPacketID.FLOAT;
            }
            else if (type == typeof(bool))
            {
                return ProtonPacketID.BOOL;
            }
            else if (type == typeof(Vector3))
            {
                return ProtonPacketID.VECTOR3;
            }
            else if (type == typeof(Quaternion))
            {
                return ProtonPacketID.QUATERNION;
            }
            else if (type == typeof(NetworkDictionary))
            {
                return ProtonPacketID.DICTIONARY;
            }
            else if (type == typeof(byte[]))
            {
                return ProtonPacketID.BYTEARRAY;
            }
            else if (type == typeof(Vector2))
            {
                return ProtonPacketID.BYTEARRAY;
            }
            return 255;
        }
    }
}