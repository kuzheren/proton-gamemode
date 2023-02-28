using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using Proton.Packet.Serialization;
using Proton.Packet.ID;

namespace Proton.Stream
{
    public class ProtonStream
    {
        public List<byte> Bytes = new List<byte>();
        public short ReadOffset = 0;

        public void WriteByte(byte value)
        {
            byte castedValue = (byte) value;
            Bytes.Add(castedValue);
        }
        public void WriteBytes(byte[] values)
        {
            foreach (byte value in (byte[]) values)
            {
                WriteByte(value);
            }
        }
        public void WriteInt16(short value)
        {
            short castedValue = (short) value;
            byte[] shortBytes = BitConverter.GetBytes(castedValue);
            for (int i = 0; i < shortBytes.Length; i++)
            {
                WriteByte(shortBytes[i]);
            }
        }
        public void WriteUInt16(ushort value)
        {
            ushort castedValue = (ushort) value;
            byte[] ushortBytes = BitConverter.GetBytes(castedValue);
            for (int i = 0; i < ushortBytes.Length; i++)
            {
                WriteByte(ushortBytes[i]);
            }
        }
        public void WriteInt32(int value)
        {
            int castedValue = (int) value;
            byte[] longBytes = BitConverter.GetBytes(castedValue);
            for (int i = 0; i < longBytes.Length; i++)
            {
                WriteByte(longBytes[i]);
            }
        }
        public void WriteUInt32(uint value)
        {
            uint castedValue = (uint) value;
            byte[] ulongBytes = BitConverter.GetBytes(castedValue);
            for (int i = 0; i < ulongBytes.Length; i++)
            {
                WriteByte(ulongBytes[i]);
            }
        }
        public void WriteFloat(float value)
        {
            byte[] floatBytes = BitConverter.GetBytes(value);
            for (int i = 0; i < floatBytes.Length; i++)
            {
                WriteByte(floatBytes[i]);
            }
        }
        public void WriteString(string value)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(value);
            ushort length = (ushort) stringBytes.Length;

            WriteUInt16(length);
            for (int i = 0; i < length; i++)
            {
                WriteByte(stringBytes[i]);
            }
        }
        public void WriteBool(bool value)
        {
            WriteByte((byte)  (value ? 1 : 0));
        }
        public void WriteVector3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }
        public void WriteQuaternion(Quaternion value)
        {
            WriteFloat(value[0]);
            WriteFloat(value[1]);
            WriteFloat(value[2]);
            WriteFloat(value[3]);
        }
        public void WriteBytearray(byte[] array)
        {
            WriteUInt32((uint) array.Length);
            WriteBytes(array);
        }

        public T Read<T>()
        {
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)ReadBool();
            }
            else if (typeof(T) == typeof(byte))
            {
                return (T)(object)ReadByte();
            }
            else if (typeof(T) == typeof(ushort))
            {
                return (T)(object)ReadUInt16();
            }
            else if (typeof(T) == typeof(short))
            {
                return (T)(object)ReadInt16();
            }
            else if (typeof(T) == typeof(uint))
            {
                return (T)(object)ReadUInt32();
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)ReadInt32();
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)ReadFloat();
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)ReadString();
            }
            else if (typeof(T) == typeof(Vector3))
            {
                return (T)(object)ReadVector3();
            }
            else if (typeof(T) == typeof(Quaternion))
            {
                return (T)(object)ReadQuaternion();
            }
            else if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)ReadBytearray();
            }
            else
            {
                throw new InvalidOperationException("Invalid type provided for reading from ProtonStream.");
            }
        }
        public byte ReadByte()
        {
            ReadOffset++;
            return Bytes[ReadOffset - 1];
        }
        public byte[] ReadBytes(uint ammount)
        {
            List<byte> result = new List<byte>();
            for (int i = 0; i < (int) ammount; i++)
            {
                result.Add(ReadByte());
            }
            return result.ToArray();
        }
        public short ReadInt16()
        {
            List<byte> shortBytes = new List<byte>();
            for (int i = 0; i < 2; i++)
            {
                shortBytes.Add(ReadByte());
            }
            return BitConverter.ToInt16(shortBytes.ToArray(), 0);
        }
        public ushort ReadUInt16()
        {
            List<byte> ushortBytes = new List<byte>();
            for (int i = 0; i < 2; i++)
            {
                ushortBytes.Add(ReadByte());
            }
            return BitConverter.ToUInt16(ushortBytes.ToArray(), 0);
        }
        public int ReadInt32()
        {
            List<byte> intBytes = new List<byte>();
            for (int i = 0; i < 4; i++)
            {
                intBytes.Add(ReadByte());
            }
            return BitConverter.ToInt32(intBytes.ToArray(), 0);
        }
        public uint ReadUInt32()
        {
            List<byte> uintBytes = new List<byte>();
            for (int i = 0; i < 4; i++)
            {
                uintBytes.Add(ReadByte());
            }
            return BitConverter.ToUInt32(uintBytes.ToArray(), 0);
        }
        public float ReadFloat()
        {
            List<byte> floatBytes = new List<byte>();
            for (int i = 0; i < 4; i++)
            {
                floatBytes.Add(ReadByte());
            }
            return BitConverter.ToSingle(floatBytes.ToArray(), 0);
        }
        public string ReadString()
        {
            ushort length = ReadUInt16();
            List<byte> stringBytes = new List<byte>();

            for (int i = 0; i < length; i++)
            {
                stringBytes.Add(ReadByte());
            }
    
            return Encoding.UTF8.GetString(stringBytes.ToArray());
        }
        public bool ReadBool()
        {
            return ReadByte() == 1;
        }
        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }
        public Quaternion ReadQuaternion()
        {
            Quaternion quaternion = new Quaternion();
            quaternion[0] = ReadFloat();
            quaternion[1] = ReadFloat();
            quaternion[2] = ReadFloat();
            quaternion[3] = ReadFloat();
            return quaternion;
        }
        public byte[] ReadBytearray()
        {
            uint length = ReadUInt32();
            return ReadBytes(length);
        }

        public void WriteNetworkValue(NetworkValue networkValue)
        {
            byte valueType = networkValue.type;
            object value = networkValue.value;

            WriteByte(valueType);
            if (valueType == ProtonPacketID.BOOL)
            {
                WriteBool((bool) value);
            }
            else if (valueType == ProtonPacketID.BYTE)
            {
                WriteByte((byte) value);
            }
            else if (valueType == ProtonPacketID.UINT16)
            {
                WriteUInt16((ushort) value);
            }
            else if (valueType == ProtonPacketID.INT16)
            {
                WriteInt16((short) value);
            }
            else if (valueType == ProtonPacketID.UINT32)
            {
                WriteUInt32((uint) value);
            }
            else if (valueType == ProtonPacketID.INT32)
            {
                WriteInt32((int) value);
            }
            else if (valueType == ProtonPacketID.FLOAT)
            {
                WriteFloat((float) value);
            }
            else if (valueType == ProtonPacketID.STRING)
            {
                WriteString((string) value);
            }
            else if (valueType == ProtonPacketID.VECTOR3)
            {
                WriteVector3((Vector3) value);
            }
            else if (valueType == ProtonPacketID.QUATERNION)
            {
                WriteQuaternion((Quaternion) value);
            }
            else if (valueType == ProtonPacketID.BYTEARRAY)
            {
                WriteBytearray((byte[]) value);
            }
        }
        public NetworkValue ReadNetworkValue()
        {
            byte valueType = ReadByte();
            object value = null;
            
            if (valueType == ProtonPacketID.BOOL)
            {
                value = Read<bool>();
            }
            else if (valueType == ProtonPacketID.BYTE)
            {
                value = Read<byte>();
            }
            else if (valueType == ProtonPacketID.UINT16)
            {
                value = Read<ushort>();
            }
            else if (valueType == ProtonPacketID.INT16)
            {
                value = Read<short>();
            }
            else if (valueType == ProtonPacketID.UINT32)
            {
                value = Read<uint>();
            }
            else if (valueType == ProtonPacketID.INT32)
            {
                value = Read<int>();
            }
            else if (valueType == ProtonPacketID.FLOAT)
            {
                value = Read<float>();
            }
            else if (valueType == ProtonPacketID.STRING)
            {
                value = Read<string>();
            }
            else if (valueType == ProtonPacketID.VECTOR3)
            {
                value = Read<Vector3>();
            }
            else if (valueType == ProtonPacketID.QUATERNION)
            {
                value = Read<Quaternion>();
            }
            else if (valueType == ProtonPacketID.BYTEARRAY)
            {
                value = Read<byte[]>();
            }

            return new NetworkValue(valueType, value);
        }

        public void WriteNetworkDictionary(NetworkDictionary networkDictionary)
        {
            Dictionary<string, NetworkValue> dictionary = networkDictionary.dictionary;

            WriteUInt16((ushort) dictionary.Count);
            foreach (string key in dictionary.Keys)
            {
                WriteString(key);
                WriteNetworkValue(dictionary[key]);
            }
        }
        public NetworkDictionary ReadNetworkDictionary()
        {
            NetworkDictionary resultDictionary = new NetworkDictionary();

            ushort valuesAmmount = ReadUInt16();
            for (int i = 0; i < (int) valuesAmmount; i++)
            {
                resultDictionary.dictionary[ReadString()] = ReadNetworkValue();
            }

            return resultDictionary;
        }
    }
}