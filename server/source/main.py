import socket, time, traceback, importlib.util, sys
from threading import Thread

from protonstream import *
from enums import *
from packetserialization import *
from structures import *
from utils import *
from networkdictionary import *

spec = importlib.util.spec_from_file_location("gamemode", "gamemode.py")
gamemode = importlib.util.module_from_spec(spec)
try:
    spec.loader.exec_module(gamemode)
except FileNotFoundError:
    logError("Gamemode file not found!")
    input("Press any key to quit...\n")
    sys.exit()

spec = importlib.util.spec_from_file_location("config", "config.py")
config = importlib.util.module_from_spec(spec)
try:
    spec.loader.exec_module(config)
except FileNotFoundError:
    logError("Config file not found!")
    input("Press any key to quit...\n")
    sys.exit()

class Room:
    def __init__(self):
        self.server = None
        self.active = False
        self.players = []
        self.name = None
        self.mapName = None
        self.maxPlayers = None
        self.password = None
        self.customRoomParameters = NetworkDictionary()
        self.joinCode = None
        self.isOpen = None
        self.isVisible = None

    def start(self):
        self.active = True
        Thread(target=self.updatePlayersInfoThread).start()

    def updatePlayersInfoThread(self):
        while self.active:
            time.sleep(5)
            self.updatePlayersInfo()

    def closeRoom(self):
        self.active = False

    #################################################

    def addPlayer(self, player):
        self.players.append(player)
        self.sendDataToAllPlayers("sendPlayerJoined", player)
        self.updateRoomInfo()

    def kickPlayer(self, player, reason="kick"):
        if player == None:
            return
        if not player in self.players:
            return

        player.sendKickNotification(reason)
        self.server.disconnectPlayer(player, reason)

    def removePlayer(self, player):
        if player == None:
            return
        if not player in self.players:
            return
            
        self.players.remove(player)

        self.sendDataToAllPlayers("sendPlayerQuited", player)

    def updateRoomInfo(self):
        self.sendDataToAllPlayers("sendRoomInfo", self)

    def updatePlayersInfo(self):
        self.sendDataToAllPlayers("sendPlayersList", self.players, update=True)

    def sendRoomRPC(self, RPCName, senderID, networkValues):
        self.sendDataToAllPlayers("sendRPC", RPCName, senderID, networkValues)

    #################################################

    def sendDataToAllPlayers(self, functionName, *args, **kwargs):
        for player in self.players:
            getattr(player, functionName)(*args, **kwargs)

    #################################################

class Player:
    def __init__(self):
        self.active = False
        self.server = None
        self.connection = None
        self.address = None
        self.lastPingTime = None
        self.lastPongTime = None
        self.authKey = None
        self.exceptedResponse = None
        self.currentRoom = None
        self.roomSynced = False

        self.ID = None
        self.nickname = ""
        self.ping = 0.0
        self.customPlayerParameters = NetworkDictionary()
        self.properties = {}

        self.packetQueue = []

    def __eq__(self, other):
        if isinstance(other, Player):
            return self.ID == other.ID
        return False

    def __ne__(self, other):
        return not self.__eq__(other)

    def __str__(self):
        return f"[{self.ID}]({self.nickname})"

    def __repr__(self):
        return str(self)

    def start(self):
        self.active = True
        Thread(target=self.listenThread).start()
        Thread(target=self.pingThread).start()
        Thread(target=self.sendThread).start()

    def stop(self):
        self.active = False

    def pingThread(self):
        while self.active:
            time.sleep(1)
            self.sendPingPacket()
            if self.lastPingTime == None or self.lastPongTime == None:
                continue
            if self.lastPingTime - self.lastPongTime > 10:
                self.server.disconnectPlayer(self, PING_TIMEOUT)

    def processPacket(self, PS):
        packetID = PS.readByte()
        if packetID == AUTH_KEY_REQUEST:
            self.authKey = generateAuthKey()
            self.exceptedResponse = generateAuthKeyResponse(self.authKey).upper()
            authKeyStructure = AuthKey(self.authKey)
            authKeySerializer = Serializer(AUTH_KEY, authKeyStructure)
            self.sendPacket(authKeySerializer.protonStream)
        elif packetID == CONNECTION_REQUEST:
            connectionRequestStructure = Deserializer(PS, ConnectionRequest()).structure
            if connectionRequestStructure.authKeyResponse.value != self.exceptedResponse:
                self.sendServerError(AUTHKEY_BAD_RESPONSE)
                self.server.disconnectPlayer(self, AUTHKEY_WRONG_RESPONSE)
                return

            self.nickname = connectionRequestStructure.nickname.value

            if gamemode.OnConnectionRequest(self) == False:
                return

            log(f"New player connected: {self.nickname}")

            self.sendConnectionRequestAcceptedPacket()
            self.sendPlayerClassPacket(self, local=True)
        elif packetID == JOIN_ROOM_BY_NAME_REQUEST:
            if self.currentRoom != None:
                self.sendServerError(ALREADY_IN_ROOM)
                return

            joinRoomRequestStructure = Deserializer(PS, JoinRoomRequest()).structure
            roomName = joinRoomRequestStructure.roomName.value
            password = joinRoomRequestStructure.password.value
            joinedRoom = self.server.room

            if joinedRoom == None:
                return
            
            if joinedRoom.password != password:
                return

            if gamemode.OnRoomJoinRequest(self, joinedRoom) == False:
                return

            if joinedRoom != None:
                Thread(target=self.joinToRoom, args=(joinedRoom, )).start()
        elif packetID == PLAYER_CLASS_INFO:
            if self.currentRoom == None:
                return

            updatedInfoStructure = Deserializer(PS, PlayerInfo()).structure
            self.nickname = updatedInfoStructure.nickname.value
            self.customPlayerParameters = updatedInfoStructure.customPlayerParameters.value

            if gamemode.OnPlayerInfoUpdated(self, updatedInfoStructure) == False:
                return

            self.currentRoom.updatePlayersInfo()
        elif packetID == UPDATE_ROOM_INFO:
            if self.currentRoom == None:
                self.sendServerError(NOT_IN_ROOM)
                return

            currentRoom = self.currentRoom
            updatedInfoStructure = Deserializer(PS, RoomInfo()).structure
            currentRoom.name = updatedInfoStructure.roomName.value
            currentRoom.mapName = updatedInfoStructure.mapName.value
            currentRoom.maxPlayers = updatedInfoStructure.maxPlayers.value
            currentRoom.password = updatedInfoStructure.password.value
            currentRoom.customRoomParameters = updatedInfoStructure.customRoomParameters.value
            currentRoom.isOpen = updatedInfoStructure.isOpen.value
            currentRoom.isVisible = updatedInfoStructure.isVisible.value

            if gamemode.OnRoomInfoUpdated(self, updatedInfoStructure) == False:
                return

            self.currentRoom.updateRoomInfo()
        elif packetID == CHAT_MESSAGE:
            if self.currentRoom == None:
                self.sendServerError(NOT_IN_ROOM)
                return

            chatMessageInfo = Deserializer(PS, ChatMessage()).structure
            message = chatMessageInfo.chatMessage.value

            if message[0] == "/":
                fullCommandsText = message[1:]
                arguments = fullCommandsText.split()
                argumentsString = ' '.join(arguments[1:])
                argumentsList = argumentsString.split()
                gamemode.OnChatCommand(self, arguments[0], argumentsString, argumentsList)
                return

            gamemode.OnChatMessage(self, message)

    def processRPC(self, PS):
        deserializer = RPCDeserializer(PS)
        if deserializer.targetID == SERVER:
            pass
        elif deserializer.targetID == ROOM:
            if self.currentRoom != None:
                self.currentRoom.sendRoomRPC(deserializer.RPCName, self.ID, deserializer.networkValues)
        elif deserializer.targetID == GLOBAL:
            for player in self.server.players:
                player.sendRPC(deserializer.RPCName, self.ID, deserializer.networkValues)
        else:
            targetPlayer = self.server.getPlayerByID(deserializer.targetID)
            if targetPlayer != None:
                targetPlayer.sendRPC(deserializer.RPCName, self.ID, deserializer.networkValues)

        gamemode.OnReceiveRPC(self, deserializer.RPCName, deserializer.targetID, deserializer.networkValues, deserializer.values)
        if hasattr(gamemode, f"RPC_{deserializer.RPCName}"):
            rpcCallback = getattr(gamemode, f"RPC_{deserializer.RPCName}")
            rpcCallback(self, deserializer.targetID, deserializer.networkValues, deserializer.values)

    #################################################

    def joinToRoom(self, joinedRoom):
        log(f"Player {self.nickname} joined room {joinedRoom.name}")
        self.currentRoom = joinedRoom
        joinedRoom.addPlayer(self)
        time.sleep(1)
        self.sendPlayersList(joinedRoom.players)
        time.sleep(1)
        self.sendJoinRoomRequestAccepted(joinedRoom)
        self.roomSynced = True

    #################################################

    def sendPingPacket(self):
        self.lastPingTime = time.time()

        PS = ProtonStream()
        PS.writeByte(PING)
        self.sendProtonStream(PS)

    def sendConnectionRequestAcceptedPacket(self):
        connectionAcceptedStructure = ConnectionRequestAccepted(config.VERSION, config.GAME_VERSION, config.SERVER_NAME)
        connectionAcceptedSerializer = Serializer(CONNECTION_REQUEST_ACCEPTED, connectionAcceptedStructure)
        self.sendPacket(connectionAcceptedSerializer.protonStream)

    def sendJoinRoomRequestAccepted(self, room):
        joinRoomStructure = RoomInfo(room.name, room.mapName, len(room.players), room.maxPlayers, room.password, room.customRoomParameters, room.isOpen, room.isVisible, room.joinCode)
        joinRoomSerializer = Serializer(JOIN_ROOM_REQUEST_ACCEPTED, joinRoomStructure)
        self.sendPacket(joinRoomSerializer.protonStream)

    def sendPlayerClassPacket(self, player, local=False):
        def sendInfo():
            playerStructure = PlayerInfo(player.ID, player.nickname, player.ping, player.customPlayerParameters)
            playerStructureSerializer = Serializer(PLAYER_CLASS_INFO, playerStructure)

            playerCustomInfo = NetworkDictionary({"local": NetworkValue(BOOL, local), "isList": NetworkValue(BOOL, False)})
            self.sendPacket(playerStructureSerializer.protonStream, playerCustomInfo)

        if local == False and self.roomSynced == False:
            def waitForSend():
                while self.roomSynced == False:
                    time.sleep(0.1)
                sendInfo()
            Thread(target=waitForSend).start()
        else:
            sendInfo()

    def sendPlayersList(self, playersList, update=False):
        if update == True and self.roomSynced == False:
            return

        playersStructuresList = []
        for player in playersList:
            playersStructure = PlayerInfo(player.ID, player.nickname, player.ping, player.customPlayerParameters)
            playersStructuresList.append(playersStructure)
        playersInfo = Serializer(PLAYER_CLASS_INFO, None, playersStructuresList)

        playersListCustomInfo = NetworkDictionary({"isList": NetworkValue(BOOL, True), "update": NetworkValue(BOOL, update)})
        self.sendPacket(playersInfo.protonStream, playersListCustomInfo)

    def sendRoomInfo(self, room):
        roomStructure = RoomInfo(room.name, room.mapName, len(room.players), room.maxPlayers, room.password, room.customRoomParameters, room.isOpen, room.isVisible, room.joinCode)
        roomInfo = Serializer(UPDATE_ROOM_INFO, roomStructure)

        self.sendPacket(roomInfo.protonStream)

    def sendPlayerClassRemovePacket(self, removedPlayer):
        removedPlayerStructure = RemovedPlayerInfo(removedPlayer.ID)
        removedPlayerSerializer = Serializer(REMOVE_PLAYER_CLASS_INFO, removedPlayerStructure)
        self.sendPacket(removedPlayerSerializer.protonStream)

    def sendChatMessage(self, chatMessage):
        chatMessageStructure = ChatMessage(chatMessage)
        chatMessageSerializer = Serializer(CHAT_MESSAGE, chatMessageStructure)
        self.sendPacket(chatMessageSerializer.protonStream)

    def sendServerError(self, errorTuple):
        errorStructure = ServerError(errorTuple[0], errorTuple[1])
        errorSerializer = Serializer(SERVER_ERROR, errorStructure)
        self.sendPacket(errorSerializer.protonStream)

    def sendKickNotification(self, reason):
        kickMessageStructure = KickNotification(reason)
        kickMessageSerializer = Serializer(KICK, kickMessageStructure)
        self.sendPacket(kickMessageSerializer.protonStream)

    #################################################

    def sendPlayerJoined(self, newPlayer):
        if newPlayer == self:
            return
        self.sendPlayerClassPacket(newPlayer)

    def sendPlayerQuited(self, quitedPlayer):
        if quitedPlayer == self:
            return
        self.sendPlayerClassRemovePacket(quitedPlayer)

    #################################################

    def sendRPC(self, RPCName, senderID, networkValues):
        serializer = RPCSerializer(RPCName, senderID, networkValues)
        self.sendRPCPacket(serializer.protonStream)

    def listenThread(self):
        while self.active:
            try:
                PS = ProtonStream()
                dataSizeBytes = self.connection.recv(4)
                PS.setBytes(dataSizeBytes)
                firstSize = PS.readUInt32()

                totalData = b""
                while len(totalData) < firstSize:
                    dataFromClient = self.connection.recv(firstSize - len(totalData))
                    totalData += dataFromClient

                data = totalData
                PS = ProtonStream()
                PS.setBytes(data)
                self.processData(PS)
            except ConnectionResetError:
                self.server.disconnectPlayer(self, CRASH)
            except ConnectionAbortedError:
                self.server.disconnectPlayer(self, QUIT)
            except Exception as error:
                pass
                #logError(traceback.format_exc())

    def processData(self, PS):
        dataID = PS.readByte()
        if dataID == PACKET:
            self.processPacket(PS)
        elif dataID == RPC:
            self.processRPC(PS)
        elif dataID == PONG:
            self.lastPongTime = time.time()
            self.ping = (self.lastPongTime - self.lastPingTime) * 1000

    def sendPacket(self, PS, networkDictionary=None):
        if networkDictionary == None:
            networkDictionary = NetworkDictionary()
        sendPS = ProtonStream()
        sendPS.writeByte(PACKET)
        sendPS.writeNetworkDictionary(networkDictionary)
        sendPS.writeBytes(PS.getBytes())
        self.sendProtonStream(sendPS)

    def sendRPCPacket(self, PS):
        sendPS = ProtonStream()
        sendPS.writeByte(RPC)
        sendPS.writeBytes(PS.getBytes())
        self.sendProtonStream(sendPS)

    def sendProtonStream(self, PS):
        sendPS = ProtonStream()
        sendPS.writeUInt32(len(PS.getBytes()))
        sendPS.writeBytes(PS.getBytes())
        self.packetQueue.append(sendPS)

    def sendThread(self):
        while self.active:
            time.sleep(0.05)
            if len(self.packetQueue) == 0:
                continue

            PS = self.packetQueue.pop(0)
            
            try:
                self.connection.send(bytes(PS.getBytes()))
            except ConnectionResetError:
                self.server.disconnectPlayer(self, CRASH)
            except ConnectionAbortedError:
                self.server.disconnectPlayer(self, QUIT)
            except Exception as error:
                logError(traceback.format_exc())

class Server:
    def __init__(self):
        self.players = []
        self.room = self.createMainRoom()
        self.address = None
        self.socket = None

    #################################################

    def createMainRoom(self):
        room = Room()
        room.server = self
        room.customRoomParameters = NetworkDictionary()
        room.mapName = "Game"
        room.maxPlayers = config.MAX_PLAYERS
        room.password = ""
        room.isOpen = True
        room.isVisible = True
        generatedRoomCode = generateRoomCode()
        room.joinCode = generatedRoomCode
        room.name = f"Room {config.SERVER_NAME}"
        room.start()
        return room

    def closeRoom(self, room):
        if not room in self.rooms:
            return
        log(f"Room closed: {room.name}")
        room.closeRoom()
        self.rooms.remove(room)

    #################################################

    def start(self, ip, port):
        self.address = (ip, port)
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.socket.bind(self.address)
        gamemode.start(self)
        self.socket.listen(config.MAX_PLAYERS)

        Thread(target=self.listenForConnections).start()

        log(f"\033[92mProton Gamemode Server\033[0m version {config.VERSION} started! Address: {self.address}")

    def disconnectPlayer(self, player, reason):
        if player == None:
            return
        if not player in self.players:
            return
            
        player.stop()
        if player.currentRoom != None:
            player.currentRoom.removePlayer(player)

        self.players.remove(player)
        
        log(f"Player {player.nickname} disconnected. Reason: {reason}")
        gamemode.OnClientDisconnected(player)

    def listenForConnections(self):
        while True:
            try:
                connection, address = self.socket.accept()
                log(f"New TCP connection from {address}")

                newPlayer = Player()
                newPlayer.connection = connection
                newPlayer.server = self
                newPlayer.address = address
                newPlayer.ID = generateUniqueID()

                if gamemode.OnClientConnected(address) == False:
                    return

                newPlayer.start()

                self.players.append(newPlayer)
            except:
                logError(traceback.format_exc())

    def getPlayerByID(self, ID):
        for player in self.players:
            if player.ID == ID:
                return player
        return None

if __name__ == "__main__":
    server = Server()
    server.start(config.IP, config.PORT)