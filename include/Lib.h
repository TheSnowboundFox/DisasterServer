#ifndef LIB_H
#define LIB_H
#include <enet/enet.h>
#include <io/Threads.h>
#include <DyList.h>
#include <Packet.h>

typedef enum
{
	DR_FAILEDTOCONNECT,
	DR_KICKEDBYHOST,
	DR_BANNEDBYHOST,
	DR_VERMISMATCH,
	DR_SERVERTIMEOUT,
	DR_PACKETSNOTRECV,
	DR_GAMESTARTED,
	DR_AFKTIMEOUT,
	DR_LOBBYFULL,
	DR_RATELIMITED,
	DR_SHUTDOWN,
	DR_IPINUSE,

	DR_DONTREPORT = 254,
	DR_OTHER = 255
} DisconnectReason;

struct Server;
struct PeerData;
typedef struct
{
	struct PeerData*	peer;
	String				ip_addr;
	bool				is_exe;
	int8_t				character;
} PeerInfo;

bool			disaster_init 	 (void);
int				disaster_run 	 (void);
void 			disaster_shutdown(void);
void            disaster_reboot  (void);

// API functions
struct Server*	disaster_get					(int);
int				disaster_count					(void);
bool			disaster_server_lock			(struct Server* server);
bool			disaster_server_unlock			(struct Server* server);
uint8_t			disaster_server_state			(struct Server* server);
bool			disaster_server_ban				(struct Server* server, uint16_t);
bool			disaster_server_op				(struct Server* server, uint16_t);
bool			disaster_server_timeout			(struct Server* server, uint16_t, double);
bool			disaster_server_peer			(struct Server*, int, PeerInfo*);
bool			disaster_server_peer_disconnect	(struct Server*, uint16_t, DisconnectReason, const char*);
int				disaster_server_peer_count		(struct Server*);
int				disaster_server_peer_ingame		(struct Server*);
int8_t			disaster_game_map				(struct Server*);
double			disaster_game_time				(struct Server*);
uint16_t		disaster_game_time_sec			(struct Server*);

#endif
