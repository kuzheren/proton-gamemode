from enums import *

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

def RPC_Example(targetID, networkValues, values):
    pass

def OnReceiveRPC(rpcName, targetID, networkValues, values):
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

def OnChatCommand(player, command):
    pass

###########################################
# functions

def SetPlayerProperty(player, propertyName, property):
    player.properties[propertyName] = property

def GetPlayerProperty(player, propertyName):
    if not propertyName in player.properties:
        return None
    return player.properties[propertyName]

def AddChatMessage(player, message):
    player.sendChatMessage(message)

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
    AddChatMessage(player, "Hello!")