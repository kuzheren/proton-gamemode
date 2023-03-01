from protonstream import *
from enums import *

class Serializer:
    def __init__(self, packetID=None, structure=None, structureList=None):
        self.packetID = packetID
        self.structure = structure
        self.protonStream = ProtonStream()

        if packetID != None:
            self.protonStream.writeByte(self.packetID)
        if structure != None:
            self.writeStructureToProtonStream(structure)
        if structureList != None:
            self.writeStructureListToProtonStream(structureList)

    def writeStructureToProtonStream(self, structure):
        values = structure.__dict__
        for key in values:
            networkValue = values[key]
            type = networkValue.type
            value = networkValue.value

            if type == BOOL:
                self.protonStream.writeBool(value)
            elif type == BYTE:
                self.protonStream.writeByte(value)
            elif type == UINT16:
                self.protonStream.writeUInt16(value)
            elif type == INT16:
                self.protonStream.writeInt16(value)
            elif type == UINT32:
                self.protonStream.writeUInt32(value)
            elif type == INT32:
                self.protonStream.writeInt32(value)
            elif type == FLOAT:
                self.protonStream.writeFloat(value)
            elif type == STRING:
                self.protonStream.writeString(value)
            elif type == DICTIONARY:
                self.protonStream.writeNetworkDictionary(value)
            elif type == VECTOR3:
                self.protonStream.writeVector3(value)
            elif type == VECTOR2:
                self.protonStream.writeVector2(value)
            elif type == QUATERNION:
                self.protonStream.writeQuaternion(value)
            elif type == BYTEARRAY:
                self.protonStream.writeBytearray(value)

    def writeStructureListToProtonStream(self, structureList):
        self.protonStream.writeUInt16(len(structureList))
        for structure in structureList:
            self.writeStructureToProtonStream(structure)

class Deserializer:
    def __init__(self, PS, structureExample):
        self.structure = self.readStructureFromProtonStream(PS, structureExample)

    def readStructureFromProtonStream(self, PS, structure):
        values = structure.__dict__
        for key in values:
            networkValue = values[key]
            type = networkValue.type

            if type == BOOL:
                setattr(structure, key, NetworkValue(type, PS.readBool()))
            elif type == BYTE:
                setattr(structure, key, NetworkValue(type, PS.readByte()))
            elif type == UINT16:
                setattr(structure, key, NetworkValue(type, PS.readUInt16()))
            elif type == INT16:
                setattr(structure, key, NetworkValue(type, PS.readInt16()))
            elif type == UINT32:
                setattr(structure, key, NetworkValue(type, PS.readUInt32()))
            elif type == INT32:
                setattr(structure, key, NetworkValue(type, PS.readInt32()))
            elif type == FLOAT:
                setattr(structure, key, NetworkValue(type, PS.readFloat()))
            elif type == STRING:
                setattr(structure, key, NetworkValue(type, PS.readString()))
            elif type == DICTIONARY:
                dictionary = PS.readNetworkDictionary()
                setattr(structure, key, NetworkValue(type, dictionary))
            elif type == VECTOR3:
                setattr(structure, key, NetworkValue(type, PS.readVector3()))
            elif type == VECTOR2:
                setattr(structure, key, NetworkValue(type, PS.readVector2()))
            elif type == QUATERNION:
                setattr(structure, key, NetworkValue(type, PS.readQuaternion()))
            elif type == BYTEARRAY:
                setattr(structure, key, NetworkValue(type, PS.readBytearray()))
        return structure

class RPCDeserializer:
    def __init__(self, PS):
        values, networkValues, RPCName, targetID = self.deserializeRPC(PS)
        self.networkValues = networkValues
        self.values = values
        self.RPCName = RPCName
        self.targetID = targetID

    def deserializeRPC(self, PS):
        values = []
        networkValues = []
        targetID = PS.readUInt32()
        RPCName = PS.readString()
        argumentsCount = PS.readUInt16()

        for i in range(argumentsCount):
            value = PS.readNetworkValue()
            networkValues.append(value)
            values.append(value.value)

        return values, networkValues, RPCName, targetID

class RPCSerializer:
    def __init__(self, RPCName, targetID, networkValues):
        self.RPCName = RPCName
        self.targetID = targetID
        self.networkValues = networkValues
        self.protonStream = self.serializeRPC(RPCName, targetID, networkValues)

    def serializeRPC(self, RPCName, targetID, networkValues):
        PS = ProtonStream()
        PS.writeUInt32(targetID)
        PS.writeString(RPCName)
        PS.writeUInt16(len(networkValues))
        for networkValue in networkValues:
            PS.writeNetworkValue(networkValue)

        return PS