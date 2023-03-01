import struct
from utils import constrain
from enums import *
from networkvalue import *
from networkdictionary import *
from customtypes import *

class ProtonStream:
    def __init__(self):
        self.bytes = []
        self.readOffset = 0

    def setBytes(self, bytes):
        self.bytes = list(bytes)
        self.readOffset = 0

    def getBytes(self):
        return bytes(self.bytes)

    def writeByte(self, value):
        value = value & 0xFF
        self.bytes.append(value)

    def writeBytes(self, values):
        values = list(values)
        for value in values:
            self.writeByte(value)

    def writeInt16(self, value):
        value = constrain(value, -0x7FFF, 0x7FFF)
        shortBytes = (value).to_bytes(2, byteorder="little", signed=True)
        for i in range(2):
            self.writeByte(shortBytes[i])

    def writeUInt16(self, value):
        value = constrain(value, 0, 0xFFFF)
        shortBytes = (value).to_bytes(2, byteorder="little", signed=False)
        for i in range(2):
            self.writeByte(shortBytes[i])

    def writeInt32(self, value):
        value = constrain(value, -0x7FFFFFFF, 0x7FFFFFFF)
        longBytes = (value).to_bytes(4, byteorder="little", signed=True)
        for i in range(4):
            self.writeByte(longBytes[i])

    def writeUInt32(self, value):
        value = constrain(value, 0, 0xFFFFFFFF)
        longBytes = (value).to_bytes(4, byteorder="little", signed=False)
        for i in range(4):
            self.writeByte(longBytes[i])

    def writeFloat(self, value):
        floatBytes = bytes(struct.pack("f", value))
        for i in range(4):
            self.writeByte(floatBytes[i])

    def writeString(self, value):
        stringBytes = value.encode("utf-8", "replace")

        stringLength = len(stringBytes)
        if stringLength > 0xFFFF:
            stringLength = 0xFFFF

        self.writeUInt16(stringLength)
        for i in range(stringLength):
            self.writeByte(stringBytes[i])

    def writeBool(self, value):
        self.writeByte(1 if value else 0)

    def writeBytearray(self, values):
        values = list(values)
        self.writeUInt32(len(values))
        self.writeBytes(values)

    def writeVector3(self, value):
        self.writeFloat(value.x)
        self.writeFloat(value.y)
        self.writeFloat(value.z)

    def writeVector2(self, value):
        self.writeFloat(value.x)
        self.writeFloat(value.y)

    def writeQuaternion(self, value):
        self.writeFloat(value.x)
        self.writeFloat(value.y)
        self.writeFloat(value.z)
        self.writeFloat(value.w)

    def readByte(self):
        self.readOffset += 1
        return self.bytes[self.readOffset - 1]

    def readBytes(self, ammount):
        readedBytes = []
        for i in range(ammount):
            readedBytes.append(self.readByte())
        return readedBytes

    def readInt16(self):
        shortBytes = []
        for i in range(2):
            shortBytes.append(self.readByte())
        return int.from_bytes(shortBytes, byteorder="little", signed=True)

    def readUInt16(self):
        shortBytes = []
        for i in range(2):
            shortBytes.append(self.readByte())
        return int.from_bytes(shortBytes, byteorder="little", signed=False)

    def readInt32(self):
        intBytes = []
        for i in range(4):
            intBytes.append(self.readByte())
        return int.from_bytes(intBytes, byteorder="little", signed=True)

    def readUInt32(self):
        intBytes = []
        for i in range(4):
            intBytes.append(self.readByte())
        return int.from_bytes(intBytes, byteorder="little", signed=False)

    def readFloat(self):
        floatBytes = []
        for i in range(4):
            floatBytes.append(self.readByte())
        return struct.unpack("f", bytes(floatBytes))[0]

    def readString(self):
        stringLength = self.readUInt16()
        stringBytes = []
        for i in range(stringLength):
            stringBytes.append(self.readByte())
        return bytes(stringBytes).decode("utf-8")

    def readBool(self):
        return self.readByte() == 1

    def readBytearray(self):
        length = self.readUInt32()
        return list(self.readBytes(length))

    def readVector3(self):
        return Vector3(self.readFloat(), self.readFloat(), self.readFloat())

    def readVector2(self):
        return Vector2(self.readFloat(), self.readFloat())

    def readQuaternion(self):
        return Quaternion(self.readFloat(), self.readFloat(), self.readFloat())

    def writeNetworkValue(self, networkValue):
        valueType = networkValue.type
        value = networkValue.value
        
        self.writeByte(valueType)
        if valueType == BOOL:
            self.writeBool(value)
        elif valueType == BYTE:
            self.writeByte(value)
        elif valueType == UINT16:
            self.writeUInt16(value)
        elif valueType == INT16:
            self.writeInt16(value)
        elif valueType == UINT32:
            self.writeUInt32(value)
        elif valueType == INT32:
            self.writeInt32(value)
        elif valueType == FLOAT:
            self.writeFloat(value)
        elif valueType == STRING:
            self.writeString(value)
        elif valueType == BYTEARRAY:
            self.writeBytearray(value)
        elif valueType == VECTOR3:
            self.writeVector3(value)
        elif valueType == VECTOR2:
            self.writeVector2(value)
        elif valueType == QUATERNION:
            self.writeQuaternion(value)

    def readNetworkValue(self):
        valueType = self.readByte()
        value = None

        if valueType == BOOL:
            value = self.readBool()
        elif valueType == BYTE:
            value = self.readByte()
        elif valueType == UINT16:
            value = self.readUInt16()
        elif valueType == INT16:
            value = self.readInt16()
        elif valueType == UINT32:
            value = self.readUInt32()
        elif valueType == INT32:
            value = self.readInt32()
        elif valueType == FLOAT:
            value = self.readFloat()
        elif valueType == STRING:
            value = self.readString()
        elif valueType == BYTEARRAY:
            value = self.readBytearray()
        elif valueType == VECTOR3:
            value = self.readVector3()
        elif valueType == VECTOR2:
            value = self.readVector2()
        elif valueType == QUATERNION:
            value = self.readQuaternion()

        return NetworkValue(valueType, value)

    def writeNetworkDictionary(self, networkDictionary):
        dictionary = networkDictionary.dictionary
        self.writeUInt16(len(dictionary))
        for key in dictionary:
            self.writeString(key)
            self.writeNetworkValue(dictionary[key])

    def readNetworkDictionary(self):
        dictionarySize = self.readUInt16()
        dictionary = {}
        for i in range(dictionarySize):
            key = self.readString()
            dictionary[key] = self.readNetworkValue()

        return NetworkDictionary(dictionary)