from enums import *
from networkvalue import *
from protonstream import *
from customtypes import *

server = None

def log(text):
    print("[Gamemode]:", text)

def logError(text):
    print("\033[91m[Gamemode Error]:\033[93m", text)

def logWarning(text):
    print("\033[93m[Gamemode Warning]:\033[0m", text)

def start(init):
    global server
    server = init
    OnGamemodeStarted()

###########################################

def RPC_Example(sender, targetID, networkValues, values):
    pass

def OnReceiveRPC(sender, rpcName, targetID, networkValues, values):
    pass

###########################################
# callbacks

def OnGamemodeStarted():
    log("Proton Gamemode started!")

def OnClientConnected(adress):
    return True

def OnClientDisconnected(player):
    pass

def OnConnectionRequest(player):
    return True

def OnRoomJoinRequest(player, joinedRoom):
    #HelloWorld(player) # test
    return True

def OnPlayerInfoUpdated(player, updatedInfo):
    return True

def OnRoomInfoUpdated(player, updatedInfo):
    return True

def OnChatMessage(player, message):
    formattedMessage = f"{player.nickname}: {message}"
    player.currentRoom.sendDataToAllPlayers("sendChatMessage", formattedMessage)

def OnChatCommand(player, command, argumentsLine, argumentsArray):
    pass

###########################################
# functions

def SetPlayerProperty(player, propertyName, property):
    player.properties[propertyName] = property

def GetPlayerProperty(player, propertyName):
    if not propertyName in player.properties:
        return None
    return player.properties[propertyName]

def SendChatMessage(player, message):
    player.sendChatMessage(message)

def SendGlobalChatMessage(message):
    server.room.sendDataToAllPlayers("sendChatMessage", message)

def KickPlayer(player, reason):
    if player.currentRoom == None:
        player.server.disconnectPlayer(player, reason)
    else:
        player.currentRoom.kickPlayer(player, reason)

def SendRPC(player, rpcName, values):
    player.sendRPC(rpcName, SERVER, values)

###########################################
# custom gamemode

def HelloWorld(player):
    SendChatMessage(player, "Hello!")
