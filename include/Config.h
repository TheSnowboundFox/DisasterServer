#ifndef CONFIG_H
#define CONFIG_H

#include <cJSON.h>
#include <io/Threads.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdbool.h>

#define CONFIG_FILE "Config.json"

typedef struct
{
    uint16_t    port;
    uint16_t    server_count;
    uint16_t    target_version;
    uint16_t    ping_limit;
    char        message_of_the_day[256];
    bool        log_debug;
    bool        log_to_file;
    bool        palette_anticheat;
    bool        zone_anticheat;
    bool        distance_anticheat;
    bool        ability_anticheat;
    bool        pride;
    uint8_t     lobby_start_timer;
    bool        apply_textchat_fixes;
    bool        use_results;
    bool        use_mapvote;
    uint8_t     mapvote_timer;
    bool        map_list[21];
    Mutex       map_list_lock;
    bool        charselect_mod_unlocked;
    uint8_t     charselect_timer;
    uint8_t     maximum_players_per_lobby;
    uint8_t     lobby_timeout_timer;
    uint8_t     sudden_death_timer;
    uint8_t     ring_appearance_timer;
    uint8_t     respawn_time;
    uint8_t     escape_time;
    bool        act9_walls;
    uint8_t     thunder_timer_offset;
    uint8_t     thunder_timer;
    bool        pf_bring_spawn;
    bool        rmz_easy_mode;
    uint16_t	ycr_gas_delay;
    uint8_t     votekick_cooldown;
    bool        disable_timer;
    bool        spawn_rings;
    bool        nap_snowballs;
    uint8_t     tc_acid_delay;
    bool        authoritarian_mode;
	uint16_t	shocking_delay;
    uint8_t     shocking_warning;
    uint8_t     shocking_time;
    bool        ip_validation;
    bool        ban_ip;
    bool        ban_udid;
    bool        ban_nickname;
    bool        overhell;
    uint8_t     red_ring_chance;
    uint8_t     demonization_percentage;
    uint8_t     results_timer;
    bool        allow_foreign_characters;
    bool        exclude_last_map;
    uint8_t     stalactites_timer;
    uint8_t     stalactites_timer_offset;
    uint8_t     spike_timer;
    char        server_location[64];
    uint8_t     switch_timer;
    uint8_t     switch_timer_chase;
    uint8_t     switch_warning_timer;
    uint8_t     switch_warning_timer_chase;
    uint8_t     hddoor_toggle_delay;
    uint8_t     speedbox_timer;
    uint8_t     speedbox_timer_offset;
    int8_t      tails_projectile_speed;
    uint8_t     tails_projectile_timeout_timer;
    uint8_t     ice_regeneration_timer;
    bool        singleplayer;
	uint16_t	player_maximum_errors;
	uint8_t	    op_default_level;
    uint8_t     eye_recharge_strength;
    uint8_t     eye_recharge_timer;
    uint8_t     eye_use_cost;
} Config;

extern Config g_config;

bool config_init(void);
bool config_save(void);

#endif
