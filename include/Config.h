#ifndef CONFIG_H
#define CONFIG_H

#include <cJSON.h>
#include <io/Threads.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdbool.h>

#define CONFIG_FILE "Config.json"
#define PLAYER_DATA_DIR "Player_Data/"
#define BANNED_IPS_FILE PLAYER_DATA_DIR "Banned_IPs.json"
#define BANNED_UDIDS_FILE PLAYER_DATA_DIR "Banned_UDIDs.json"
#define BANNED_NICKNAMES_FILE PLAYER_DATA_DIR "Banned_Nicknames.json"
#define OPERATORS_FILE PLAYER_DATA_DIR "Operators.json"
#define TIMEOUTS_FILE PLAYER_DATA_DIR "Timeouts.json"

typedef struct
{
	uint16_t port;
	uint16_t server_count;
	uint16_t target_version;
	uint16_t ping_limit;
	char 	message_of_the_day[256];
	bool	log_debug;
	bool	log_to_file;
	bool	palette_anticheat;
	bool	zone_anticheat;
	bool	distance_anticheat;
	bool	ability_anticheat;
	bool	pride;
	uint8_t	lobby_start_timer;
    bool	apply_textchat_fixes;
	bool	use_results;
	bool	use_mapvote;
	uint8_t	mapvote_timer;
	bool	map_list[20];
	Mutex	map_list_lock;
	bool	charselect_mod_unlocked;
	uint8_t	charselect_timer;
	uint8_t	maximum_players_per_lobby;
	uint8_t	lobby_timeout_timer;
	uint8_t	sudden_death_timer;
	uint8_t	ring_appearance_timer;
	uint8_t	respawn_time;
	uint8_t	escape_time;
	bool	act9_walls;
    uint8_t	hills_thunder_timer;
    uint8_t	hills_thunder_timer_offset;
    bool	np_switch_warning;
    bool	pf_bring_spawn;
    bool	rmz_easy_mode;
    uint16_t	ycr_gas_delay;
	uint8_t	votekick_cooldown;
	bool	disable_timer;
	bool	spawn_rings;
	bool	nap_snowballs;
	bool	tc_acid_smoke;
	bool	authoritarian_mode;
	uint16_t	shocking_delay;
	uint8_t	shocking_warning;
	uint8_t	shocking_time;
	bool	ip_validation;
	bool	ban_ip;
	bool	ban_udid;
	bool	ban_nickname;
	bool	overhell;
	uint8_t	red_ring_chance;
	bool	demonic_madness;
	uint8_t	results_timer;
	bool	singleplayer;
	bool	allow_foreign_characters;
	bool	exclude_last_map;
	uint8_t	stalactites_timer;
    uint8_t	stalactites_timer_offset;
	uint8_t	spike_timer;
	char 	server_location[64];
} Config;

extern Config g_config;
extern cJSON* g_banned_ips;
extern cJSON* g_banned_udids;
extern cJSON* g_banned_nicknames;
extern cJSON* g_timeouts;
extern cJSON* g_ops;
extern Mutex g_banMut;
extern Mutex g_timeoutMut;
extern Mutex g_opMut;

bool config_init(void);
bool config_save(void);

bool ban_add(const char* nickname, const char* udid, const char* ip);
bool ban_revoke(const char* udid, const char* ip, const char* nickname);
bool ban_check(const char* udid, const char* ip, const char* nickname, bool* result);

bool timeout_set(const char* nickname, const char* udid, const char* ip, uint64_t timestamp);
bool timeout_revoke(const char* udid, const char* ip);
bool timeout_check(const char* udid, const char* ip, uint64_t* result);

bool op_add(const char* nickname, const char* ip);
bool op_revoke(const char* ip);
bool op_check(const char* ip, bool* result);

bool collection_save(const char* file, cJSON* value);

#endif
