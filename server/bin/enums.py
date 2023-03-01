#data identificators
PACKET =                                   30
RPC =                                      31
PING =                                     32
PONG =                                     33

#packets identificators
AUTH_KEY_REQUEST =                         0
AUTH_KEY =                                 1
CONNECTION_REQUEST =                       2
CONNECTION_REQUEST_ACCEPTED =              3
JOIN_ROOM_REQUEST_ACCEPTED =               5
PLAYER_CLASS_INFO =                        6
JOIN_ROOM_BY_NAME_REQUEST =                9
REMOVE_PLAYER_CLASS_INFO =                 13
CHANGED_HOST_INFO =                        14
UPDATE_ROOM_INFO =                         15
CHAT_MESSAGE =                             20
SERVER_ERROR =                             21
KICK =                                     22

#types identificators
BOOL =                                     0
BYTE =                                     1
UINT16 =                                   2
INT16 =                                    3
UINT32 =                                   4
INT32 =                                    5
FLOAT =                                    6
STRING =                                   7
ARRAY =                                    10
DICTIONARY =                               11
BYTEARRAY =                                12

#quit reasons
CRASH =                                    (0, "crash")
QUIT =                                     (1, "quit")
AUTHKEY_WRONG_RESPONSE =                   (2, "authkey response fail")
TIMEOUT =                                  (3, "TCP timeout/crash")
PING_TIMEOUT =                             (4, "ping timeout")

#rpc targets
SERVER =                                   0
ROOM =                                     1
GLOBAL =                                   3

#errors
AUTHKEY_BAD_RESPONSE =                     (1, "Неверный ключ аутентификации")
ALREADY_IN_ROOM =                          (2, "Невозможно совершить данную операцию, так как игрок уже подключен к комнате")
NOT_IN_ROOM =                              (3, "Невозможно совершить данную операцию, так как игрок не подключен к комнате")