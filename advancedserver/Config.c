#include <Config.h>
#include <Log.h>
#include <cJSON.h>
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <io/File.h>
#include <io/Dir.h>
#include <io/Threads.h>

Config g_config = {
    .networking = {
        .port = 8606,
        .server_count = 1,
        .target_version = 1101
    },
    .pairing = {
        .maximum_players_per_lobby = 7,
        .ip_validation = true,
        .ping_limit = 250,
        .player_maximum_errors = 30000
    },
    .logging = {
        .log_debug = false,
        .log_to_file = false
    },
    .moderation = {
        .ban_ip = true,
        .ban_udid = true,
        .ban_nickname = false,
        .op_default_level = 1
    },
    .lobby_misc = {
        .message_of_the_day = "\\mods are disallowed on this server",
        .lobby_timeout_timer = 25,
        .lobby_start_timer = 5,
        .votekick_cooldown = 30,
        .apply_textchat_fixes = false,
        .authoritarian_mode = false,
        .lobby_ready_required_percentage = 100,
        .kick_unready_before_starting = true,
        .server_location = "Saint Petersburg",
        .hosts_name = "That Arctic Furry"
    },
    .map_selection = {
        .use_mapvote = true,
        .exclude_last_map = true,
        .mapvote_timer = 30,
        .map_list = { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false }
    },
    .character_selection = {
        .charselect_mod_unlocked = false,
        .charselect_timer = 30,
        .allow_foreign_characters = false
    },
    .gameplay = {
        .respawn_time = 30,
        .sudden_death_timer = 120,
        .ring_appearance_timer = 60,
        .escape_time = 50,
        .demonization_percentage = 50,
        .exe_camp_penalty = true,
        .anonymous_mode = false,
        .enable_achievements = true,
        .enable_sounds = true,
        .rmz_easy_mode = false,
        .entities_misc = {
            .global = {
                .rings = {
                    .spawn_rings = true,
                    .red_ring_chance = 11
                },
                .spikes = {
                    .timer = 2
                },
            },
            .character_specific = {
                .tails = {
                    .projectile_speed = 14,
                    .projectile_timeout_timer = 5
                }
            },
            .map_specific = {
                .act9_walls = true,
                .pf_bring_spawn = true,
                .ycr_gas_delay = 6,
                .tc_acid_delay = 4,
                .hddoor_toggle_delay = 10,
                .limb_city = {
                    .chain = {
                        .shocking_delay = 8,
                        .shocking_warning = 2,
                        .shocking_time = 2
                    },
                    .eye = {
                        .eye_recharge_strength = 10,
                        .eye_recharge_timer = 2,
                        .eye_use_cost = 20
                    }
                },
                .not_perfect = {
                    .switch_timer = 5,
                    .switch_timer_chase = 3,
                    .switch_warning_timer = 15,
                    .switch_warning_timer_chase = 2
                },
                .kind_and_fair = {
                    .speedbox_timer = 25,
                    .speedbox_timer_offset = 5
                },
                .nasty_paradise = {
                    .nap_snowballs = true,
                    .ice_regeneration_timer = 15
                },
                .dark_tower = {
                    .stalactites_timer = 25,
                    .stalactites_timer_offset = 5
                },
                .hills = {
                    .thunder_timer = 15,
                    .thunder_timer_offset = 5
                }
            }
        },
        .anticheat = {
            .palette_anticheat = true,
            .zone_anticheat = true,
            .distance_anticheat = true,
            .ability_anticheat = true
        },
        .banana = {
            .disable_timer = false,
            .singleplayer = false
        },
        .gmcycle = {
            .overhell = false
        },
        .banana = {
            .disable_timer = false,
            .singleplayer = false
        },
        .gmcycle = {
            .overhell = false
        },
    },
    .results_misc = {
        .use_results = true,
        .results_timer = 15,
        .pride = true
    }
};

bool config_init(void)
{
	MutexCreate(g_config.map_list_lock);
	
	// Try to open config
	FILE* file = fopen(CONFIG_FILE, "r");
	if (!file)
	{
		RAssert(config_save());

		// Reopen
		file = fopen(CONFIG_FILE, "r");
		if (!file)
		{
			Warn("Failed to save default config file properly!");
            return false;
		}
	}

    char buffer[4096] = { 0 };
    size_t len = fread(buffer, 1, 4096, file);
	fclose(file);

	cJSON* json = cJSON_ParseWithLength(buffer, len);
	if (!json)
	{
		Err("Failed to parse %s: %s", CONFIG_FILE, cJSON_GetErrorPtr());
		return false;
	}
	else
		Debug("%s loaded.", CONFIG_FILE);

    cJSON* networking = cJSON_GetObjectItem(json, "networking");
    g_config.networking.port = cJSON_GetObjectItem(networking, "port")->valueint;
    g_config.networking.server_count = cJSON_GetObjectItem(networking, "server_count")->valueint;
    g_config.networking.target_version = cJSON_GetObjectItem(networking, "target_version")->valueint;

    cJSON* pairing = cJSON_GetObjectItem(json, "pairing");
    g_config.pairing.maximum_players_per_lobby = cJSON_GetObjectItem(pairing, "maximum_players_per_lobby")->valueint;
    g_config.pairing.ip_validation = cJSON_IsTrue(cJSON_GetObjectItem(pairing, "ip_validation"));
    g_config.pairing.ping_limit = cJSON_GetObjectItem(pairing, "ping_limit")->valueint;
    g_config.pairing.player_maximum_errors = cJSON_GetObjectItem(pairing, "player_maximum_errors")->valueint;

    cJSON* logging = cJSON_GetObjectItem(json, "logging");
    g_config.logging.log_debug = cJSON_IsTrue(cJSON_GetObjectItem(logging, "log_debug"));
    g_config.logging.log_to_file = cJSON_IsTrue(cJSON_GetObjectItem(logging, "log_to_file"));

    cJSON* moderation = cJSON_GetObjectItem(json, "moderation");
    g_config.moderation.ban_ip = cJSON_IsTrue(cJSON_GetObjectItem(moderation, "ban_ip"));
    g_config.moderation.ban_udid = cJSON_IsTrue(cJSON_GetObjectItem(moderation, "ban_udid"));
    g_config.moderation.ban_nickname = cJSON_IsTrue(cJSON_GetObjectItem(moderation, "ban_nickname"));
    g_config.moderation.op_default_level = cJSON_GetObjectItem(moderation, "op_default_level")->valueint;

    cJSON* lobby_misc = cJSON_GetObjectItem(json, "lobby_misc");
    snprintf(g_config.lobby_misc.message_of_the_day, 80, "%s", cJSON_GetStringValue(cJSON_GetObjectItem(lobby_misc, "message_of_the_day")));
    g_config.lobby_misc.lobby_timeout_timer = cJSON_GetObjectItem(lobby_misc, "lobby_timeout_timer")->valueint;
    g_config.lobby_misc.lobby_start_timer = cJSON_GetObjectItem(lobby_misc, "lobby_start_timer")->valueint;
    g_config.lobby_misc.votekick_cooldown = cJSON_GetObjectItem(lobby_misc, "votekick_cooldown")->valueint;
    snprintf(g_config.lobby_misc.server_location, 64, "%s", cJSON_GetStringValue(cJSON_GetObjectItem(lobby_misc, "server_location")));
    g_config.lobby_misc.apply_textchat_fixes = cJSON_IsTrue(cJSON_GetObjectItem(lobby_misc, "apply_textchat_fixes"));
    g_config.lobby_misc.authoritarian_mode = cJSON_IsTrue(cJSON_GetObjectItem(lobby_misc, "authoritarian_mode"));
    g_config.lobby_misc.lobby_ready_required_percentage = cJSON_GetObjectItem(lobby_misc, "lobby_ready_required_percentage")->valueint;
    g_config.lobby_misc.kick_unready_before_starting = cJSON_IsTrue(cJSON_GetObjectItem(lobby_misc, "kick_unready_before_starting"));

    cJSON* map_selection = cJSON_GetObjectItem(json, "map_selection");
    g_config.map_selection.use_mapvote = cJSON_IsTrue(cJSON_GetObjectItem(map_selection, "use_mapvote"));
    g_config.map_selection.exclude_last_map = cJSON_IsTrue(cJSON_GetObjectItem(map_selection, "exclude_last_map"));
    g_config.map_selection.mapvote_timer = cJSON_GetObjectItem(map_selection, "mapvote_timer")->valueint;
    cJSON* map_list = cJSON_GetObjectItem(map_selection, "map_list");
    if (cJSON_IsArray(map_list))
    {
        for (int i = 0; i < 21; ++i)
        {
            g_config.map_selection.map_list[i] = cJSON_IsTrue(cJSON_GetArrayItem(map_list, i));
        }
    }

    cJSON *character_selection = cJSON_GetObjectItem(json, "character_selection");
    g_config.character_selection.charselect_mod_unlocked = cJSON_IsTrue(cJSON_GetObjectItem(character_selection, "charselect_mod_unlocked"));
    g_config.character_selection.allow_foreign_characters = cJSON_IsTrue(cJSON_GetObjectItem(character_selection, "allow_foreign_characters"));
    g_config.character_selection.charselect_timer = cJSON_GetObjectItem(character_selection, "charselect_timer")->valueint;

    cJSON *gameplay = cJSON_GetObjectItem(json, "gameplay");
    g_config.gameplay.respawn_time = cJSON_GetObjectItem(gameplay, "respawn_time")->valueint;
    g_config.gameplay.sudden_death_timer = cJSON_GetObjectItem(gameplay, "sudden_death_timer")->valueint;
    g_config.gameplay.ring_appearance_timer = cJSON_GetObjectItem(gameplay, "ring_appearance_timer")->valueint;
    g_config.gameplay.escape_time = cJSON_GetObjectItem(gameplay, "escape_time")->valueint;
    g_config.gameplay.demonization_percentage = cJSON_GetObjectItem(gameplay, "demonization_percentage")->valueint;
    g_config.gameplay.exe_camp_penalty = cJSON_IsTrue(cJSON_GetObjectItem(gameplay, "exe_camp_penalty"));
    g_config.gameplay.anonymous_mode = cJSON_IsTrue(cJSON_GetObjectItem(gameplay, "anonymous_mode"));
    g_config.gameplay.enable_achievements = cJSON_IsTrue(cJSON_GetObjectItem(gameplay, "enable_achievements"));
    g_config.gameplay.enable_sounds = cJSON_IsTrue(cJSON_GetObjectItem(gameplay, "enable_sounds"));
    g_config.gameplay.rmz_easy_mode = cJSON_IsTrue(cJSON_GetObjectItem(gameplay, "rmz_easy_mode"));

    cJSON* anticheat = cJSON_GetObjectItem(gameplay, "anticheat");
    g_config.gameplay.anticheat.palette_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "palette_anticheat"));
    g_config.gameplay.anticheat.zone_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "zone_anticheat"));
    g_config.gameplay.anticheat.distance_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "distance_anticheat"));
    g_config.gameplay.anticheat.ability_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "ability_anticheat"));

    cJSON *banana = cJSON_GetObjectItem(gameplay, "banana");
    g_config.gameplay.banana.disable_timer = cJSON_IsTrue(cJSON_GetObjectItem(banana, "disable_timer"));
    g_config.gameplay.banana.singleplayer = cJSON_IsTrue(cJSON_GetObjectItem(banana, "singleplayer"));

    cJSON *gmcycle = cJSON_GetObjectItem(gameplay, "gmcycle");
    g_config.gameplay.gmcycle.overhell = cJSON_IsTrue(cJSON_GetObjectItem(gmcycle, "overhell"));

    cJSON* entities_misc = cJSON_GetObjectItem(gameplay, "entities_misc");

    cJSON* global = cJSON_GetObjectItem(entities_misc, "global");

    cJSON* rings = cJSON_GetObjectItem(global, "rings");
    g_config.gameplay.entities_misc.global.rings.spawn_rings = cJSON_IsTrue(cJSON_GetObjectItem(rings, "spawn_rings"));
    g_config.gameplay.entities_misc.global.rings.red_ring_chance = cJSON_GetObjectItem(rings, "red_ring_chance")->valueint;

    cJSON* spikes = cJSON_GetObjectItem(global, "spikes");
    g_config.gameplay.entities_misc.global.spikes.timer = cJSON_GetObjectItem(spikes, "timer")->valueint;

    cJSON *map_specific = cJSON_GetObjectItem(entities_misc, "map_specific");

    cJSON *limp_city = cJSON_GetObjectItem(map_specific, "limp_city");

    cJSON *chain = cJSON_GetObjectItem(limp_city, "chain");
    g_config.gameplay.entities_misc.map_specific.limb_city.chain.shocking_delay = cJSON_GetObjectItem(chain, "shocking_delay")->valueint;
    g_config.gameplay.entities_misc.map_specific.limb_city.chain.shocking_warning = cJSON_GetObjectItem(chain, "shocking_warning")->valueint;
    g_config.gameplay.entities_misc.map_specific.limb_city.chain.shocking_time = cJSON_GetObjectItem(chain, "shocking_time")->valueint;

    cJSON *eye = cJSON_GetObjectItem(limp_city, "eye");
    g_config.gameplay.entities_misc.map_specific.limb_city.eye.eye_recharge_strength = cJSON_GetObjectItem(eye, "eye_recharge_strength")->valueint;
    g_config.gameplay.entities_misc.map_specific.limb_city.eye.eye_recharge_timer = cJSON_GetObjectItem(eye, "eye_recharge_timer")->valueint;
    g_config.gameplay.entities_misc.map_specific.limb_city.eye.eye_use_cost = cJSON_GetObjectItem(eye, "eye_use_cost")->valueint;

    cJSON *not_perfect = cJSON_GetObjectItem(map_specific, "not_perfect");
    g_config.gameplay.entities_misc.map_specific.not_perfect.switch_timer = cJSON_GetObjectItem(not_perfect, "switch_timer")->valueint;
    g_config.gameplay.entities_misc.map_specific.not_perfect.switch_timer_chase = cJSON_GetObjectItem(not_perfect, "switch_timer_chase")->valueint;
    g_config.gameplay.entities_misc.map_specific.not_perfect.switch_warning_timer = cJSON_GetObjectItem(not_perfect, "switch_warning_timer")->valueint;
    g_config.gameplay.entities_misc.map_specific.not_perfect.switch_warning_timer_chase = cJSON_GetObjectItem(not_perfect, "switch_warning_timer_chase")->valueint;

    cJSON *kind_and_fair = cJSON_GetObjectItem(map_specific, "kind_and_fair");
    g_config.gameplay.entities_misc.map_specific.kind_and_fair.speedbox_timer = cJSON_GetObjectItem(kind_and_fair, "speedbox_timer")->valueint;
    g_config.gameplay.entities_misc.map_specific.kind_and_fair.speedbox_timer_offset = cJSON_GetObjectItem(kind_and_fair, "speedbox_timer_offset")->valueint;

    cJSON *nasty_paradise = cJSON_GetObjectItem(map_specific, "nasty_paradise");
    g_config.gameplay.entities_misc.map_specific.nasty_paradise.nap_snowballs = cJSON_IsTrue(cJSON_GetObjectItem(nasty_paradise, "nap_snowballs"));
    g_config.gameplay.entities_misc.map_specific.nasty_paradise.ice_regeneration_timer = cJSON_GetObjectItem(nasty_paradise, "ice_regeneration_timer")->valueint;

    cJSON *hills = cJSON_GetObjectItem(map_specific, "hills");
    g_config.gameplay.entities_misc.map_specific.hills.thunder_timer = cJSON_GetObjectItem(hills, "thunder_timer")->valueint;
    g_config.gameplay.entities_misc.map_specific.hills.thunder_timer_offset = cJSON_GetObjectItem(hills, "thunder_timer_offset")->valueint;

    cJSON *dark_tower = cJSON_GetObjectItem(map_specific, "dark_tower");
    g_config.gameplay.entities_misc.map_specific.dark_tower.stalactites_timer = cJSON_GetObjectItem(dark_tower, "stalactites_timer")->valueint;
    g_config.gameplay.entities_misc.map_specific.dark_tower.stalactites_timer_offset = cJSON_GetObjectItem(dark_tower, "stalactites_timer_offset")->valueint;

    g_config.gameplay.entities_misc.map_specific.act9_walls = cJSON_IsTrue(cJSON_GetObjectItem(map_specific, "act9_walls"));
    g_config.gameplay.entities_misc.map_specific.pf_bring_spawn = cJSON_IsTrue(cJSON_GetObjectItem(map_specific, "pf_bring_spawn"));
    g_config.gameplay.entities_misc.map_specific.ycr_gas_delay = cJSON_GetObjectItem(map_specific, "ycr_gas_delay")->valueint;
    g_config.gameplay.entities_misc.map_specific.tc_acid_delay = cJSON_GetObjectItem(map_specific, "tc_acid_delay")->valueint;
    g_config.gameplay.entities_misc.map_specific.hddoor_toggle_delay = cJSON_GetObjectItem(map_specific, "hddoor_toggle_delay")->valueint;

    cJSON *character_specific = cJSON_GetObjectItem(entities_misc, "character_specific");

    cJSON *tails = cJSON_GetObjectItem(character_specific, "tails");
    g_config.gameplay.entities_misc.character_specific.tails.projectile_speed = cJSON_GetObjectItem(tails, "projectile_speed")->valueint;
    g_config.gameplay.entities_misc.character_specific.tails.projectile_timeout_timer = cJSON_GetObjectItem(tails, "projectile_timeout_timer")->valueint;

    cJSON* results_misc = cJSON_GetObjectItem(json, "results_misc");
    g_config.results_misc.use_results = cJSON_IsTrue(cJSON_GetObjectItem(results_misc, "use_results"));
    g_config.results_misc.results_timer = cJSON_GetObjectItem(results_misc, "results_timer")->valueint;
    g_config.results_misc.pride = cJSON_IsTrue(cJSON_GetObjectItem(results_misc, "pride"));

    cJSON_Delete(json);

	return true;
}

bool config_save(void)
{
	cJSON* json = cJSON_CreateObject();
	RAssert(json);

    cJSON *networking = cJSON_CreateObject();
    cJSON_AddNumberToObject(networking, "port", g_config.networking.port);
    cJSON_AddNumberToObject(networking, "server_count", g_config.networking.server_count);
    cJSON_AddNumberToObject(networking, "target_version", g_config.networking.target_version);
    cJSON_AddItemToObject(json, "networking", networking);

    cJSON* pairing = cJSON_CreateObject();
    cJSON_AddNumberToObject(pairing, "maximum_players_per_lobby", g_config.pairing.maximum_players_per_lobby);
    cJSON_AddBoolToObject(pairing, "ip_validation", g_config.pairing.ip_validation);
    cJSON_AddNumberToObject(pairing, "ping_limit", g_config.pairing.ping_limit);
    cJSON_AddNumberToObject(pairing, "player_maximum_errors", g_config.pairing.player_maximum_errors);
    cJSON_AddItemToObject(json, "pairing", pairing);

    cJSON* logging = cJSON_CreateObject();
    cJSON_AddBoolToObject(logging, "log_debug", g_config.logging.log_debug);
    cJSON_AddBoolToObject(logging, "log_to_file", g_config.logging.log_to_file);
    cJSON_AddItemToObject(json, "logging", logging);

    cJSON* moderation = cJSON_CreateObject();
    cJSON_AddBoolToObject(moderation, "ban_ip", g_config.moderation.ban_ip);
    cJSON_AddBoolToObject(moderation, "ban_udid", g_config.moderation.ban_udid);
    cJSON_AddBoolToObject(moderation, "ban_nickname", g_config.moderation.ban_nickname);
    cJSON_AddNumberToObject(moderation, "op_default_level", g_config.moderation.op_default_level);
    cJSON_AddItemToObject(json, "moderation", moderation);

    cJSON* lobby_misc = cJSON_CreateObject();
    cJSON_AddStringToObject(lobby_misc, "message_of_the_day", g_config.lobby_misc.message_of_the_day);
    cJSON_AddNumberToObject(lobby_misc, "lobby_timeout_timer", g_config.lobby_misc.lobby_timeout_timer);
    cJSON_AddNumberToObject(lobby_misc, "lobby_start_timer", g_config.lobby_misc.lobby_start_timer);
    cJSON_AddNumberToObject(lobby_misc, "votekick_cooldown", g_config.lobby_misc.votekick_cooldown);
    cJSON_AddStringToObject(lobby_misc, "server_location", g_config.lobby_misc.server_location);
    cJSON_AddBoolToObject(lobby_misc, "apply_textchat_fixes", g_config.lobby_misc.apply_textchat_fixes);
    cJSON_AddBoolToObject(lobby_misc, "authoritarian_mode", g_config.lobby_misc.authoritarian_mode);
    cJSON_AddNumberToObject(lobby_misc, "lobby_ready_required_percentage", g_config.lobby_misc.lobby_ready_required_percentage);
    cJSON_AddBoolToObject(lobby_misc, "kick_unready_before_starting", g_config.lobby_misc.kick_unready_before_starting);
    cJSON_AddItemToObject(json, "lobby_misc", lobby_misc);

    cJSON* map_selection = cJSON_CreateObject();
    cJSON_AddBoolToObject(map_selection, "use_mapvote", g_config.map_selection.use_mapvote);
    cJSON_AddBoolToObject(map_selection, "exclude_last_map", g_config.map_selection.exclude_last_map);
    cJSON_AddNumberToObject(map_selection, "mapvote_timer", g_config.map_selection.mapvote_timer);
    cJSON* map_list = cJSON_CreateArray();
    for (int i = 0; i < 21; ++i) {
        cJSON_AddItemToArray(map_list, cJSON_CreateBool(g_config.map_selection.map_list[i]));
    }
    cJSON_AddItemToObject(map_selection, "map_list", map_list);
    cJSON_AddItemToObject(json, "map_selection", map_selection);

    cJSON* character_selection = cJSON_CreateObject();
    cJSON_AddBoolToObject(character_selection, "charselect_mod_unlocked", g_config.character_selection.charselect_mod_unlocked);
    cJSON_AddBoolToObject(character_selection, "allow_foreign_characters", g_config.character_selection.allow_foreign_characters);
    cJSON_AddNumberToObject(character_selection, "charselect_timer", g_config.character_selection.charselect_timer);
    cJSON_AddItemToObject(json, "character_selection", character_selection);

    cJSON* gameplay = cJSON_CreateObject();
    cJSON_AddNumberToObject(gameplay, "respawn_time", g_config.gameplay.respawn_time);
    cJSON_AddNumberToObject(gameplay, "sudden_death_timer", g_config.gameplay.sudden_death_timer);
    cJSON_AddNumberToObject(gameplay, "ring_appearance_timer", g_config.gameplay.ring_appearance_timer);
    cJSON_AddNumberToObject(gameplay, "escape_time", g_config.gameplay.escape_time);
    cJSON_AddNumberToObject(gameplay, "demonization_percentage", g_config.gameplay.demonization_percentage);
    cJSON_AddBoolToObject(gameplay, "exe_camp_penalty", g_config.gameplay.exe_camp_penalty);
    cJSON_AddBoolToObject(gameplay, "anonymous_mode", g_config.gameplay.anonymous_mode);
    cJSON_AddBoolToObject(gameplay, "enable_achievements", g_config.gameplay.enable_achievements);
    cJSON_AddBoolToObject(gameplay, "enable_sounds", g_config.gameplay.enable_sounds);
    cJSON_AddBoolToObject(gameplay, "rmz_easy_mode", g_config.gameplay.rmz_easy_mode);
    cJSON_AddItemToObject(json, "gameplay", gameplay);

    cJSON* anticheat = cJSON_CreateObject();
    cJSON_AddBoolToObject(anticheat, "palette_anticheat", g_config.gameplay.anticheat.palette_anticheat);
    cJSON_AddBoolToObject(anticheat, "zone_anticheat", g_config.gameplay.anticheat.zone_anticheat);
    cJSON_AddBoolToObject(anticheat, "distance_anticheat", g_config.gameplay.anticheat.distance_anticheat);
    cJSON_AddBoolToObject(anticheat, "ability_anticheat", g_config.gameplay.anticheat.ability_anticheat);
    cJSON_AddItemToObject(gameplay, "anticheat", anticheat);

    cJSON *banana = cJSON_CreateObject();
    cJSON_AddBoolToObject(banana, "disable_timer", g_config.gameplay.banana.disable_timer);
    cJSON_AddBoolToObject(banana, "singleplayer", g_config.gameplay.banana.singleplayer);
    cJSON_AddItemToObject(gameplay, "banana", banana);

    cJSON *gmcycle = cJSON_CreateObject();
    cJSON_AddBoolToObject(gmcycle, "overhell", g_config.gameplay.gmcycle.overhell);
    cJSON_AddItemToObject(gameplay, "gmcycle", gmcycle);

    cJSON* entities_misc = cJSON_CreateObject();

    cJSON* global = cJSON_CreateObject();

    cJSON* rings = cJSON_CreateObject();
    cJSON_AddBoolToObject(rings, "spawn_rings", g_config.gameplay.entities_misc.global.rings.spawn_rings);
    cJSON_AddNumberToObject(rings, "red_ring_chance", g_config.gameplay.entities_misc.global.rings.red_ring_chance);
    cJSON_AddItemToObject(global, "rings", rings);

    cJSON* spikes = cJSON_CreateObject();
    cJSON_AddNumberToObject(spikes, "timer", g_config.gameplay.entities_misc.global.spikes.timer);
    cJSON_AddItemToObject(global, "spikes", spikes);

    cJSON_AddItemToObject(entities_misc, "global", global);

    cJSON *map_specific = cJSON_CreateObject();

    cJSON *limp_city = cJSON_CreateObject();

    cJSON *chain = cJSON_CreateObject();
    cJSON_AddNumberToObject(chain, "shocking_delay", g_config.gameplay.entities_misc.map_specific.limb_city.chain.shocking_delay);
    cJSON_AddNumberToObject(chain, "shocking_warning", g_config.gameplay.entities_misc.map_specific.limb_city.chain.shocking_warning);
    cJSON_AddNumberToObject(chain, "shocking_time", g_config.gameplay.entities_misc.map_specific.limb_city.chain.shocking_time);
    cJSON_AddItemToObject(limp_city, "chain", chain);

    cJSON *eye = cJSON_CreateObject();
    cJSON_AddNumberToObject(eye, "eye_recharge_strength", g_config.gameplay.entities_misc.map_specific.limb_city.eye.eye_recharge_strength);
    cJSON_AddNumberToObject(eye, "eye_recharge_timer", g_config.gameplay.entities_misc.map_specific.limb_city.eye.eye_recharge_timer);
    cJSON_AddNumberToObject(eye, "eye_use_cost", g_config.gameplay.entities_misc.map_specific.limb_city.eye.eye_use_cost);
    cJSON_AddItemToObject(limp_city, "eye", eye);

    cJSON_AddItemToObject(map_specific, "limp_city", limp_city);

    cJSON *not_perfect = cJSON_CreateObject();
    cJSON_AddNumberToObject(not_perfect, "switch_timer", g_config.gameplay.entities_misc.map_specific.not_perfect.switch_timer);
    cJSON_AddNumberToObject(not_perfect, "switch_timer_chase", g_config.gameplay.entities_misc.map_specific.not_perfect.switch_timer_chase);
    cJSON_AddNumberToObject(not_perfect, "switch_warning_timer", g_config.gameplay.entities_misc.map_specific.not_perfect.switch_warning_timer);
    cJSON_AddNumberToObject(not_perfect, "switch_warning_timer_chase", g_config.gameplay.entities_misc.map_specific.not_perfect.switch_warning_timer_chase);
    cJSON_AddItemToObject(map_specific, "not_perfect", not_perfect);

    cJSON *kind_and_fair = cJSON_CreateObject();
    cJSON_AddNumberToObject(kind_and_fair, "speedbox_timer", g_config.gameplay.entities_misc.map_specific.kind_and_fair.speedbox_timer);
    cJSON_AddNumberToObject(kind_and_fair, "speedbox_timer_offset", g_config.gameplay.entities_misc.map_specific.kind_and_fair.speedbox_timer_offset);
    cJSON_AddItemToObject(map_specific, "kind_and_fair", kind_and_fair);

    cJSON *nasty_paradise = cJSON_CreateObject();
    cJSON_AddBoolToObject(nasty_paradise, "nap_snowballs", g_config.gameplay.entities_misc.map_specific.nasty_paradise.nap_snowballs);
    cJSON_AddNumberToObject(nasty_paradise, "ice_regeneration_timer", g_config.gameplay.entities_misc.map_specific.nasty_paradise.ice_regeneration_timer);
    cJSON_AddItemToObject(map_specific, "nasty_paradise", nasty_paradise);

    cJSON *hills = cJSON_CreateObject();
    cJSON_AddNumberToObject(hills, "thunder_timer", g_config.gameplay.entities_misc.map_specific.hills.thunder_timer);
    cJSON_AddNumberToObject(hills, "thunder_timer_offset", g_config.gameplay.entities_misc.map_specific.hills.thunder_timer_offset);
    cJSON_AddItemToObject(map_specific, "hills", hills);

    cJSON *dark_tower = cJSON_CreateObject();
    cJSON_AddNumberToObject(dark_tower, "stalactites_timer", g_config.gameplay.entities_misc.map_specific.dark_tower.stalactites_timer);
    cJSON_AddNumberToObject(dark_tower, "stalactites_timer_offset", g_config.gameplay.entities_misc.map_specific.dark_tower.stalactites_timer_offset);
    cJSON_AddItemToObject(map_specific, "dark_tower", dark_tower);

    cJSON_AddBoolToObject(map_specific, "act9_walls", g_config.gameplay.entities_misc.map_specific.act9_walls);
    cJSON_AddBoolToObject(map_specific, "pf_bring_spawn", g_config.gameplay.entities_misc.map_specific.pf_bring_spawn);
    cJSON_AddNumberToObject(map_specific, "ycr_gas_delay", g_config.gameplay.entities_misc.map_specific.ycr_gas_delay);
    cJSON_AddNumberToObject(map_specific, "tc_acid_delay", g_config.gameplay.entities_misc.map_specific.tc_acid_delay);
    cJSON_AddNumberToObject(map_specific, "hddoor_toggle_delay", g_config.gameplay.entities_misc.map_specific.hddoor_toggle_delay);

    cJSON_AddItemToObject(entities_misc, "map_specific", map_specific);

    cJSON *character_specific = cJSON_CreateObject();

    cJSON *tails = cJSON_CreateObject();
    cJSON_AddNumberToObject(tails, "projectile_speed", g_config.gameplay.entities_misc.character_specific.tails.projectile_speed);
    cJSON_AddNumberToObject(tails, "projectile_timeout_timer", g_config.gameplay.entities_misc.character_specific.tails.projectile_timeout_timer);
    cJSON_AddItemToObject(character_specific, "tails", tails);

    cJSON_AddItemToObject(entities_misc, "character_specific", character_specific);

    cJSON_AddItemToObject(gameplay, "entities_misc", entities_misc);

    cJSON* results_misc = cJSON_CreateObject();
    cJSON_AddBoolToObject(results_misc, "use_results", g_config.results_misc.use_results);
    cJSON_AddNumberToObject(results_misc, "results_timer", g_config.results_misc.results_timer);
    cJSON_AddBoolToObject(results_misc, "pride", g_config.results_misc.pride);
    cJSON_AddItemToObject(json, "results_misc", results_misc);

	RAssert(collection_save(CONFIG_FILE, json));
    cJSON_Delete(json);
	return true;
}
