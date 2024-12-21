#ifndef API_H
#define API_H

bool			disaster_server_lock(struct Server* server);
bool			disaster_server_unlock(struct Server* server);
uint8_t			disaster_server_state(struct Server* server);
bool			disaster_server_ban(struct Server* server, uint16_t);
bool			disaster_server_op(struct Server* server, uint16_t);
bool			disaster_server_timeout(struct Server* server, uint16_t, double);
bool			disaster_server_peer(struct Server*, int, PeerInfo*);
bool			disaster_server_peer_disconnect(struct Server*, uint16_t, DisconnectReason, const char*);
int				disaster_server_peer_count(struct Server*);
int				disaster_server_peer_ingame(struct Server*);
int8_t			disaster_game_map(struct Server*);
double			disaster_game_time(struct Server*);
uint16_t		disaster_game_time_sec(struct Server*);

#endif
