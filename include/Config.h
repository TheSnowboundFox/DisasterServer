#ifndef CONFIG_H
#define CONFIG_H

#include <cJSON.h>
#include <io/Threads.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdbool.h>

#define CONFIG_FILE "Config.json"

struct Config;
typedef struct
{
    uint16_t    port;
    uint16_t    server_count;
    uint16_t    target_version;
} Networking;
typedef struct
{
    uint8_t     maximum_players_per_lobby;
    bool        ip_validation;
    uint16_t    ping_limit;
    uint16_t	player_maximum_errors;
} Pairing;
typedef struct
{
    bool        log_debug;
    bool        log_to_file;
} Logging;
typedef struct
{
    bool        ban_ip;
    bool        ban_udid;
    bool        ban_nickname;
    uint8_t	    op_default_level;
} Moderation;
typedef struct
{
    char        message_of_the_day[80];
    uint8_t     lobby_timeout_timer;
    uint8_t     lobby_start_timer;
    uint8_t     votekick_cooldown;
    char        server_location[64];
    bool        apply_textchat_fixes;
    bool        authoritarian_mode;
    uint8_t     lobby_ready_required_percentage;
    bool        kick_unready_before_starting;
    char        hosts_name[30];
} LobbyMisc;
typedef struct
{
    bool        use_mapvote;
    bool        exclude_last_map;
    uint8_t     mapvote_timer;
    bool        map_list[21];
} MapSelection;
typedef struct
{
    bool        charselect_mod_unlocked;
    bool        allow_foreign_characters;
    uint8_t     charselect_timer;
} CharacterSelection;
typedef struct
{
    bool        disable_timer;
    bool        singleplayer;
} Banana;
typedef struct
{
    bool        overhell;
} GMCycle;
typedef struct
{
    bool        use_results;
    uint8_t     results_timer;
    bool        pride;
    
} ResultsMisc;
typedef struct
{
    bool        palette_anticheat;
    bool        zone_anticheat;
    bool        distance_anticheat;
    bool        ability_anticheat;
} Anticheat;
typedef struct
{
    uint8_t     eye_recharge_strength;
    uint8_t     eye_recharge_timer;
    uint8_t     eye_use_cost;
} Eye;
typedef struct
{
    uint16_t	shocking_delay;
    uint8_t     shocking_warning;
    uint8_t     shocking_time;
} Chain;
typedef struct
{
    Eye         eye;
    Chain       chain;
} LimbCity;
typedef struct
{
    uint8_t     switch_timer;
    uint8_t     switch_timer_chase;
    uint8_t     switch_warning_timer;
    uint8_t     switch_warning_timer_chase;
} NotPerfect;
typedef struct
{
    uint8_t     speedbox_timer;
    uint8_t     speedbox_timer_offset;
} KindAndFair;
typedef struct
{
    bool        nap_snowballs;
    uint8_t     ice_regeneration_timer;
} NastyParadise;
typedef struct
{
    uint8_t     thunder_timer_offset;
    uint8_t     thunder_timer;
} Hills;
typedef struct
{
    uint8_t     stalactites_timer;
    uint8_t     stalactites_timer_offset;
} DarkTower;
typedef struct
{
    uint8_t     stalactites_timer;
    uint8_t     stalactites_timer_offset;
} FartZone;
typedef struct
{
    bool        act9_walls;
    bool        pf_bring_spawn;
    uint16_t	ycr_gas_delay;
    uint8_t     tc_acid_delay;
    uint8_t     hddoor_toggle_delay;
    LimbCity    limb_city;
    NotPerfect  not_perfect;
    KindAndFair kind_and_fair;
    NastyParadise   nasty_paradise;
    Hills       hills;
    DarkTower   dark_tower;
} MapSpecific;
typedef struct
{
    int8_t      projectile_speed;
    uint8_t     projectile_timeout_timer;
} Tails;
typedef struct
{
    Tails       tails;
} CharacterSpecific;
typedef struct
{
    bool        spawn_rings;
    uint8_t     red_ring_chance;
} Rings;
typedef struct
{
    uint8_t     timer;
} Spikes;
typedef struct
{
    Rings        rings;
    Spikes      spikes;
} Global;
typedef struct
{
    Global      global;
    MapSpecific map_specific;
    CharacterSpecific   character_specific;
} EntitiesMisc;
typedef struct
{
    uint8_t     respawn_time;
    uint8_t     sudden_death_timer;
    uint8_t     ring_appearance_timer;
    uint8_t     escape_time;
    uint8_t     demonization_percentage;
    bool        exe_camp_penalty;
    bool        anonymous_mode;
    bool        enable_achievements;
    bool        enable_sounds;
    bool        rmz_easy_mode;
    EntitiesMisc    entities_misc;
    Anticheat   anticheat;
    Banana      banana;
    GMCycle     gmcycle;
} Gameplay;
typedef struct
{
    Mutex       map_list_lock;
    Networking networking;
    Pairing pairing;
    Logging logging;
    Moderation  moderation;
    LobbyMisc   lobby_misc;
    MapSelection    map_selection;
    CharacterSelection  character_selection;
    Gameplay    gameplay;
    ResultsMisc results_misc;
} Config;

extern Config g_config;

bool config_init(void);
bool config_save(void);

#endif
