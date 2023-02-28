from distutils.log import error
from operator import pos
from enums import *
from networkvalue import *

class ConnectionRequest:
    def __init__(self, nickname=None, gameVersion=None, authKeyResponse=None):
        self.nickname = NetworkValue(STRING, nickname)
        self.gameVersion = NetworkValue(STRING, gameVersion)
        self.authKeyResponse = NetworkValue(STRING, authKeyResponse)

class ConnectionRequestAccepted:
    def __init__(self, serverVersion, gameVersion):
        self.streamZone = NetworkValue(STRING, serverVersion)
        self.gameVersion = NetworkValue(STRING, gameVersion)

class AuthKey:
    def __init__(self, authKey=None):
        self.authKey = NetworkValue(STRING, authKey)

class RoomInfo:
    def __init__(self, roomName=None, mapName=None, currentPlayers=None, maxPlayers=None, password=None, customRoomParameters=None, isOpen=None, isVisible=None, roomJoinCode=None):
        self.roomName = NetworkValue(STRING, roomName)
        self.mapName = NetworkValue(STRING, mapName)
        self.currentPlayers = NetworkValue(UINT16, currentPlayers)
        self.maxPlayers = NetworkValue(UINT16, maxPlayers)
        self.password = NetworkValue(STRING, password)
        self.customRoomParameters = NetworkValue(DICTIONARY, customRoomParameters)
        self.isOpen = NetworkValue(BOOL, isOpen)
        self.isVisible = NetworkValue(BOOL, isVisible)
        self.roomJoinCode = NetworkValue(STRING, roomJoinCode)

class PlayerInfo:
    def __init__(self, ID=None, nickname=None, ping=None, customPlayerParameters=None):
        self.ID = NetworkValue(UINT32, ID)
        self.nickname = NetworkValue(STRING, nickname)
        self.ping = NetworkValue(FLOAT, ping)
        self.customPlayerParameters = NetworkValue(DICTIONARY, customPlayerParameters)

class JoinRoomRequest:
    def __init__(self, roomName=None, password=None):
        self.roomName = NetworkValue(STRING, roomName)
        self.password = NetworkValue(STRING, password)

class JoinRoomByCodeRequest:
    def __init__(self, roomCode=None):
        self.roomCode = NetworkValue(STRING, roomCode)

class RemovedPlayerInfo:
    def __init__(self, ID=None):
        self.ID = NetworkValue(UINT32, ID)

class ChangedHostInfo:
    def __init__(self, ID=None):
        self.ID = NetworkValue(UINT32, ID)

class ChatMessage:
    def __init__(self, chatMessage=None):
        self.chatMessage = NetworkValue(STRING, chatMessage)

class ServerError:
    def __init__(self, errorCode=None, errorMessage=None):
        self.errorCode = NetworkValue(INT32, errorCode)
        self.errorMessage = NetworkValue(STRING, errorMessage)

class KickNotification:
    def __init__(self, reason=None):
        self.reason = NetworkValue(STRING, reason)