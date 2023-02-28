import random, string, hashlib

def log(text):
    print("[Server]:", text)

def logError(text):
    print("\033[91m[Server Error]:\033[93m", text)

def logWarning(text):
    print("\033[93m[Server Warning]:\033[0m", text)

def generateAuthKey():
    return "".join(random.choice(string.ascii_uppercase) for i in range(16))

def generateAuthKeyResponse(authKey):
    authKey = authKey[:16]
    result = hashlib.sha256(authKey.encode()).hexdigest()[:16]
    return result

def constrain(val, minVal, maxVal):
    return min(max(val, minVal), maxVal)

def generateRoomCode():
    return "".join(random.choices("ABCDEF123456789", k=8))

def generateUniqueID():
    return random.randint(0x00000000, 0xFFFFFFFF)