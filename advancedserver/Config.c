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
    .port = 8606,
    .server_count = 1,
    .target_version = 1101,
    .ping_limit = 250,
    .message_of_the_day = "\\mods are disallowed on this server",
    .log_debug = false,
    .log_to_file = false,
    .palette_anticheat = true,
    .zone_anticheat = true,
    .distance_anticheat = true,
    .ability_anticheat = true,
    .pride = true,
    .lobby_start_timer = 5,
    .apply_textchat_fixes = false,
    .use_results = true,
    .use_mapvote = true,
    .mapvote_timer = 30,
    .map_list = { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
    .charselect_mod_unlocked = false,
    .charselect_timer = 30,
    .maximum_players_per_lobby = 7,
    .lobby_timeout_timer = 25,
    .sudden_death_timer = 120,
    .ring_appearance_timer = 60,
    .respawn_time = 30,
    .escape_time = 50,
    .act9_walls = true,
    .thunder_timer = 15,
    .thunder_timer_offset = 5,
    .pf_bring_spawn = true,
    .rmz_easy_mode = false,
    .ycr_gas_delay = 6,
    .votekick_cooldown = 30,
    .disable_timer = false,
    .spawn_rings = true,
    .nap_snowballs = true,
    .tc_acid_delay = 4,
    .authoritarian_mode = false,
    .shocking_delay = 8,
    .shocking_warning = 2,
    .shocking_time = 2,
    .ip_validation = true,
    .ban_ip = true,
    .ban_udid = true,
    .ban_nickname = false,
    .overhell = false,
    .red_ring_chance = 11,
    .demonization_percentage = 50,
    .results_timer = 15,
    .allow_foreign_characters = false,
    .exclude_last_map = true,
    .stalactites_timer = 25,
    .stalactites_timer_offset = 5,
    .spike_timer = 2,
    .server_location = "Saint Petersburg",
    .switch_timer = 5,
    .switch_timer_chase = 3,
    .switch_warning_timer = 15,
    .switch_warning_timer_chase = 2,
    .hddoor_toggle_delay = 10,
    .speedbox_timer = 25,
    .speedbox_timer_offset = 5,
    .tails_projectile_speed = 14,
    .tails_projectile_timeout_timer = 5,
    .ice_regeneration_timer = 15,
    .singleplayer = false,
    .player_maximum_errors = 30000,
    .op_default_level = 1,
    .eye_recharge_strength = 10,
    .eye_recharge_timer = 2,
    .eye_use_cost = 20
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

    char buffer[3072] = { 0 };
    size_t len = fread(buffer, 1, 3072, file);
	fclose(file);

	cJSON* json = cJSON_ParseWithLength(buffer, len);
	if (!json)
	{
		Err("Failed to parse %s: %s", CONFIG_FILE, cJSON_GetErrorPtr());
		return false;
	}
	else
		Debug("%s loaded.", CONFIG_FILE);

    cJSON *networking = cJSON_GetObjectItem(json, "networking");
    g_config.port = cJSON_GetObjectItem(networking, "port")->valueint;
    g_config.server_count = cJSON_GetObjectItem(networking, "server_count")->valueint;
    g_config.target_version = cJSON_GetObjectItem(networking, "target_version")->valueint;

    cJSON *pairing = cJSON_GetObjectItem(json, "pairing");
    g_config.maximum_players_per_lobby = cJSON_GetObjectItem(pairing, "maximum_players_per_lobby")->valueint;
    g_config.ip_validation = cJSON_IsTrue(cJSON_GetObjectItem(pairing, "ip_validation"));
    g_config.ping_limit = cJSON_GetObjectItem(pairing, "ping_limit")->valueint;
    g_config.player_maximum_errors = cJSON_GetObjectItem(pairing, "player_maximum_errors")->valueint;

    cJSON *logging = cJSON_GetObjectItem(json, "logging");
    g_config.log_debug = cJSON_IsTrue(cJSON_GetObjectItem(logging, "log_debug"));
    g_config.log_to_file = cJSON_IsTrue(cJSON_GetObjectItem(logging, "log_to_file"));

    cJSON *moderation = cJSON_GetObjectItem(json, "moderation");
    g_config.ban_ip = cJSON_IsTrue(cJSON_GetObjectItem(moderation, "ban_ip"));
    g_config.ban_udid = cJSON_IsTrue(cJSON_GetObjectItem(moderation, "ban_udid"));
    g_config.ban_nickname = cJSON_IsTrue(cJSON_GetObjectItem(moderation, "ban_nickname"));
    g_config.op_default_level = cJSON_GetObjectItem(moderation, "op_default_level")->valueint;

    cJSON *lobby_misc = cJSON_GetObjectItem(json, "lobby_misc");
    snprintf(g_config.message_of_the_day, 256, "%s", cJSON_GetStringValue(cJSON_GetObjectItem(lobby_misc, "message_of_the_day")));
    g_config.lobby_timeout_timer = cJSON_GetObjectItem(lobby_misc, "lobby_timeout_timer")->valueint;
    g_config.lobby_start_timer = cJSON_GetObjectItem(lobby_misc, "lobby_start_timer")->valueint;
    g_config.votekick_cooldown = cJSON_GetObjectItem(lobby_misc, "votekick_cooldown")->valueint;
    g_config.apply_textchat_fixes = cJSON_IsTrue(cJSON_GetObjectItem(lobby_misc, "apply_textchat_fixes"));
    g_config.authoritarian_mode = cJSON_IsTrue(cJSON_GetObjectItem(lobby_misc, "authoritarian_mode"));
    snprintf(g_config.server_location, 64, "%s", cJSON_GetStringValue(cJSON_GetObjectItem(lobby_misc, "server_location")));

    cJSON *map_selection = cJSON_GetObjectItem(json, "map_selection");
    g_config.use_mapvote = cJSON_IsTrue(cJSON_GetObjectItem(map_selection, "use_mapvote"));
    g_config.exclude_last_map = cJSON_IsTrue(cJSON_GetObjectItem(map_selection, "exclude_last_map"));
    g_config.mapvote_timer = cJSON_GetObjectItem(map_selection, "mapvote_timer")->valueint;
    cJSON *map_list = cJSON_GetObjectItem(map_selection, "map_list");
    if (cJSON_IsArray(map_list)) {
        for (int i = 0; i < 21; ++i) {
            g_config.map_list[i] = cJSON_IsTrue(cJSON_GetArrayItem(map_list, i));
        }
    }

    cJSON *character_selection = cJSON_GetObjectItem(json, "character_selection");
    g_config.charselect_mod_unlocked = cJSON_IsTrue(cJSON_GetObjectItem(character_selection, "charselect_mod_unlocked"));
    g_config.allow_foreign_characters = cJSON_IsTrue(cJSON_GetObjectItem(character_selection, "allow_foreign_characters"));
    g_config.charselect_timer = cJSON_GetObjectItem(character_selection, "charselect_timer")->valueint;

    cJSON *gameplay = cJSON_GetObjectItem(json, "gameplay");
    g_config.respawn_time = cJSON_GetObjectItem(gameplay, "respawn_time")->valueint;
    g_config.sudden_death_timer = cJSON_GetObjectItem(gameplay, "sudden_death_timer")->valueint;
    g_config.ring_appearance_timer = cJSON_GetObjectItem(gameplay, "ring_appearance_timer")->valueint;
    g_config.escape_time = cJSON_GetObjectItem(gameplay, "escape_time")->valueint;

    g_config.spawn_rings = cJSON_IsTrue(cJSON_GetObjectItem(gameplay, "spawn_rings"));
    g_config.red_ring_chance = cJSON_GetObjectItem(gameplay, "red_ring_chance")->valueint;
    g_config.demonization_percentage = cJSON_GetObjectItem(gameplay, "demonization_percentage")->valueint;
    g_config.tails_projectile_speed = cJSON_GetObjectItem(gameplay, "tails_projectile_speed")->valueint;
    g_config.tails_projectile_timeout_timer = cJSON_GetObjectItem(gameplay, "tails_projectile_timeout_timer")->valueint;

    cJSON *banana = cJSON_GetObjectItem(json, "banana");
    g_config.disable_timer = cJSON_IsTrue(cJSON_GetObjectItem(banana, "disable_timer"));
    g_config.singleplayer = cJSON_IsTrue(cJSON_GetObjectItem(banana, "singleplayer"));

    cJSON *gmcycle = cJSON_GetObjectItem(json, "gmcycle");
    g_config.overhell = cJSON_IsTrue(cJSON_GetObjectItem(gmcycle, "overhell"));

    cJSON *results_misc = cJSON_GetObjectItem(json, "results_misc");
    g_config.use_results = cJSON_IsTrue(cJSON_GetObjectItem(results_misc, "use_results"));
    g_config.results_timer = cJSON_GetObjectItem(results_misc, "results_timer")->valueint;
    g_config.pride = cJSON_IsTrue(cJSON_GetObjectItem(results_misc, "pride"));

    cJSON *anticheat = cJSON_GetObjectItem(json, "anticheat");
    g_config.palette_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "palette_anticheat"));
    g_config.zone_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "zone_anticheat"));
    g_config.distance_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "distance_anticheat"));
    g_config.ability_anticheat = cJSON_IsTrue(cJSON_GetObjectItem(anticheat, "ability_anticheat"));

    cJSON *map_specific = cJSON_GetObjectItem(json, "map_specific");

    cJSON *limp_city = cJSON_GetObjectItem(map_specific, "limp_city");

    cJSON *chain = cJSON_GetObjectItem(limp_city, "chain");
    g_config.shocking_delay = cJSON_GetObjectItem(chain, "shocking_delay")->valueint;
    g_config.shocking_warning = cJSON_GetObjectItem(chain, "shocking_warning")->valueint;
    g_config.shocking_time = cJSON_GetObjectItem(chain, "shocking_time")->valueint;

    cJSON *eye = cJSON_GetObjectItem(limp_city, "eye");
    g_config.eye_recharge_strength = cJSON_GetObjectItem(eye, "eye_recharge_strength")->valueint;
    g_config.eye_recharge_timer = cJSON_GetObjectItem(eye, "eye_recharge_timer")->valueint;
    g_config.eye_use_cost = cJSON_GetObjectItem(eye, "eye_use_cost")->valueint;

    cJSON *not_perfect = cJSON_GetObjectItem(map_specific, "not_perfect");
    g_config.switch_timer = cJSON_GetObjectItem(not_perfect, "switch_timer")->valueint;
    g_config.switch_timer_chase = cJSON_GetObjectItem(not_perfect, "switch_timer_chase")->valueint;
    g_config.switch_warning_timer = cJSON_GetObjectItem(not_perfect, "switch_warning_timer")->valueint;
    g_config.switch_warning_timer_chase = cJSON_GetObjectItem(not_perfect, "switch_warning_timer_chase")->valueint;

    cJSON *kind_and_fair = cJSON_GetObjectItem(map_specific, "kind_and_fair");
    g_config.speedbox_timer = cJSON_GetObjectItem(kind_and_fair, "speedbox_timer")->valueint;
    g_config.speedbox_timer_offset = cJSON_GetObjectItem(kind_and_fair, "speedbox_timer_offset")->valueint;

    cJSON *nasty_paradise = cJSON_GetObjectItem(map_specific, "nasty_paradise");
    g_config.nap_snowballs = cJSON_IsTrue(cJSON_GetObjectItem(nasty_paradise, "nap_snowballs"));
    g_config.ice_regeneration_timer = cJSON_GetObjectItem(nasty_paradise, "ice_regeneration_timer")->valueint;

    cJSON *hills = cJSON_GetObjectItem(map_specific, "hills");
    g_config.thunder_timer = cJSON_GetObjectItem(hills, "thunder_timer")->valueint;
    g_config.thunder_timer_offset = cJSON_GetObjectItem(hills, "thunder_timer_offset")->valueint;

    cJSON *dark_tower = cJSON_GetObjectItem(map_specific, "dark_tower");
    g_config.stalactites_timer = cJSON_GetObjectItem(dark_tower, "stalactites_timer")->valueint;
    g_config.stalactites_timer_offset = cJSON_GetObjectItem(dark_tower, "stalactites_timer_offset")->valueint;

    g_config.act9_walls = cJSON_IsTrue(cJSON_GetObjectItem(map_specific, "act9_walls"));
    g_config.pf_bring_spawn = cJSON_IsTrue(cJSON_GetObjectItem(map_specific, "pf_bring_spawn"));
    g_config.rmz_easy_mode = cJSON_IsTrue(cJSON_GetObjectItem(map_specific, "rmz_easy_mode"));
    g_config.ycr_gas_delay = cJSON_GetObjectItem(map_specific, "ycr_gas_delay")->valueint;
    g_config.nap_snowballs = cJSON_IsTrue(cJSON_GetObjectItem(map_specific, "nap_snowballs"));
    g_config.tc_acid_delay = cJSON_GetObjectItem(map_specific, "tc_acid_delay")->valueint;
    g_config.hddoor_toggle_delay = cJSON_GetObjectItem(map_specific, "hddoor_toggle_delay")->valueint;

    cJSON_Delete(json);

    if (!g_config.zone_anticheat || !g_config.ability_anticheat || !g_config.distance_anticheat)
	{
		Info(LOG_YLW "Anticheat is disabled, client modifications are allowed.");
    }

	return true;
}

bool config_save(void)
{
	cJSON* json = cJSON_CreateObject();
	RAssert(json);

    cJSON *networking = cJSON_CreateObject();
    cJSON_AddNumberToObject(networking, "port", g_config.port);
    cJSON_AddNumberToObject(networking, "server_count", g_config.server_count);
    cJSON_AddNumberToObject(networking, "target_version", g_config.target_version);
    cJSON_AddItemToObject(json, "networking", networking);

    cJSON *pairing = cJSON_CreateObject();
    cJSON_AddNumberToObject(pairing, "maximum_players_per_lobby", g_config.maximum_players_per_lobby);
    cJSON_AddBoolToObject(pairing, "ip_validation", g_config.ip_validation);
    cJSON_AddNumberToObject(pairing, "ping_limit", g_config.ping_limit);
    cJSON_AddNumberToObject(pairing, "player_maximum_errors", g_config.player_maximum_errors);
    cJSON_AddItemToObject(json, "pairing", pairing);

    cJSON *logging = cJSON_CreateObject();
    cJSON_AddBoolToObject(logging, "log_debug", g_config.log_debug);
    cJSON_AddBoolToObject(logging, "log_to_file", g_config.log_to_file);
    cJSON_AddItemToObject(json, "logging", logging);

    cJSON *moderation = cJSON_CreateObject();
    cJSON_AddBoolToObject(moderation, "ban_ip", g_config.ban_ip);
    cJSON_AddBoolToObject(moderation, "ban_udid", g_config.ban_udid);
    cJSON_AddBoolToObject(moderation, "ban_nickname", g_config.ban_nickname);
    cJSON_AddNumberToObject(moderation, "op_default_level", g_config.op_default_level);
    cJSON_AddItemToObject(json, "moderation", moderation);

    cJSON *lobby_misc = cJSON_CreateObject();
    cJSON_AddStringToObject(lobby_misc, "message_of_the_day", g_config.message_of_the_day);
    cJSON_AddNumberToObject(lobby_misc, "lobby_timeout_timer", g_config.lobby_timeout_timer);
    cJSON_AddNumberToObject(lobby_misc, "lobby_start_timer", g_config.lobby_start_timer);
    cJSON_AddNumberToObject(lobby_misc, "votekick_cooldown", g_config.votekick_cooldown);
    cJSON_AddBoolToObject(lobby_misc, "apply_textchat_fixes", g_config.apply_textchat_fixes);
    cJSON_AddBoolToObject(lobby_misc, "authoritarian_mode", g_config.authoritarian_mode);
    cJSON_AddStringToObject(lobby_misc, "server_location", g_config.server_location);
    cJSON_AddItemToObject(json, "lobby_misc", lobby_misc);

    cJSON *map_selection = cJSON_CreateObject();
    cJSON_AddBoolToObject(map_selection, "use_mapvote", g_config.use_mapvote);
    cJSON_AddBoolToObject(map_selection, "exclude_last_map", g_config.exclude_last_map);
    cJSON_AddNumberToObject(map_selection, "mapvote_timer", g_config.mapvote_timer);
    cJSON* map_list = cJSON_CreateArray();
    for (int i = 0; i < 21; ++i) {
        cJSON_AddItemToArray(map_list, cJSON_CreateBool(g_config.map_list[i]));
    }
    cJSON_AddItemToObject(map_selection, "map_list", map_list);
    cJSON_AddItemToObject(json, "map_selection", map_selection);

    cJSON *character_selection = cJSON_CreateObject();
    cJSON_AddBoolToObject(character_selection, "charselect_mod_unlocked", g_config.charselect_mod_unlocked);
    cJSON_AddBoolToObject(character_selection, "allow_foreign_characters", g_config.allow_foreign_characters);
    cJSON_AddNumberToObject(character_selection, "charselect_timer", g_config.charselect_timer);
    cJSON_AddItemToObject(json, "character_selection", character_selection);

    cJSON *gameplay = cJSON_CreateObject();
    cJSON_AddNumberToObject(gameplay, "respawn_time", g_config.respawn_time);
    cJSON_AddNumberToObject(gameplay, "sudden_death_timer", g_config.sudden_death_timer);
    cJSON_AddNumberToObject(gameplay, "ring_appearance_timer", g_config.ring_appearance_timer);
    cJSON_AddNumberToObject(gameplay, "escape_time", g_config.escape_time);
    cJSON_AddBoolToObject(gameplay, "spawn_rings", g_config.spawn_rings);
    cJSON_AddNumberToObject(gameplay, "red_ring_chance", g_config.red_ring_chance);
    cJSON_AddNumberToObject(gameplay, "demonization_percentage", g_config.demonization_percentage);
    cJSON_AddNumberToObject(gameplay, "tails_projectile_speed", g_config.tails_projectile_speed);
    cJSON_AddNumberToObject(gameplay, "tails_projectile_timeout_timer", g_config.tails_projectile_timeout_timer);
    cJSON_AddItemToObject(json, "gameplay", gameplay);

    cJSON *banana = cJSON_CreateObject();
    cJSON_AddBoolToObject(banana, "disable_timer", g_config.disable_timer);
    cJSON_AddBoolToObject(banana, "singleplayer", g_config.singleplayer);
    cJSON_AddItemToObject(json, "banana", banana);

    cJSON *gmcycle = cJSON_CreateObject();
    cJSON_AddBoolToObject(gmcycle, "overhell", g_config.overhell);
    cJSON_AddItemToObject(json, "gmcycle", gmcycle);

    cJSON *results_misc = cJSON_CreateObject();
    cJSON_AddBoolToObject(results_misc, "use_results", g_config.use_results);
    cJSON_AddNumberToObject(results_misc, "results_timer", g_config.results_timer);
    cJSON_AddBoolToObject(results_misc, "pride", g_config.pride);
    cJSON_AddItemToObject(json, "results_misc", results_misc);

    cJSON *anticheat = cJSON_CreateObject();
    cJSON_AddBoolToObject(anticheat, "palette_anticheat", g_config.palette_anticheat);
    cJSON_AddBoolToObject(anticheat, "zone_anticheat", g_config.zone_anticheat);
    cJSON_AddBoolToObject(anticheat, "distance_anticheat", g_config.distance_anticheat);
    cJSON_AddBoolToObject(anticheat, "ability_anticheat", g_config.ability_anticheat);
    cJSON_AddItemToObject(json, "anticheat", anticheat);

    cJSON *map_specific = cJSON_CreateObject();

    cJSON *limp_city = cJSON_CreateObject();

    cJSON *chain = cJSON_CreateObject();
    cJSON_AddNumberToObject(chain, "shocking_delay", g_config.shocking_delay);
    cJSON_AddNumberToObject(chain, "shocking_warning", g_config.shocking_warning);
    cJSON_AddNumberToObject(chain, "shocking_time", g_config.shocking_time);
    cJSON_AddItemToObject(limp_city, "chain", chain);

    cJSON *eye = cJSON_CreateObject();
    cJSON_AddNumberToObject(eye, "eye_recharge_strength", g_config.eye_recharge_strength);
    cJSON_AddNumberToObject(eye, "eye_recharge_timer", g_config.eye_recharge_timer);
    cJSON_AddNumberToObject(eye, "eye_use_cost", g_config.eye_use_cost);
    cJSON_AddItemToObject(limp_city, "eye", eye);

    cJSON_AddItemToObject(map_specific, "limp_city", limp_city);

    cJSON *not_perfect = cJSON_CreateObject();
    cJSON_AddNumberToObject(not_perfect, "switch_timer", g_config.switch_timer);
    cJSON_AddNumberToObject(not_perfect, "switch_timer_chase", g_config.switch_timer_chase);
    cJSON_AddNumberToObject(not_perfect, "switch_warning_timer", g_config.switch_warning_timer);
    cJSON_AddNumberToObject(not_perfect, "switch_warning_timer_chase", g_config.switch_warning_timer_chase);
    cJSON_AddItemToObject(map_specific, "not_perfect", not_perfect);

    cJSON *kind_and_fair = cJSON_CreateObject();
    cJSON_AddNumberToObject(kind_and_fair, "speedbox_timer", g_config.speedbox_timer);
    cJSON_AddNumberToObject(kind_and_fair, "speedbox_timer_offset", g_config.speedbox_timer_offset);
    cJSON_AddItemToObject(map_specific, "kind_and_fair", kind_and_fair);

    cJSON *nasty_paradise = cJSON_CreateObject();
    cJSON_AddBoolToObject(nasty_paradise, "nap_snowballs", g_config.nap_snowballs);
    cJSON_AddNumberToObject(nasty_paradise, "ice_regeneration_timer", g_config.ice_regeneration_timer);
    cJSON_AddItemToObject(map_specific, "nasty_paradise", nasty_paradise);

    cJSON *hills = cJSON_CreateObject();
    cJSON_AddNumberToObject(hills, "thunder_timer", g_config.thunder_timer);
    cJSON_AddNumberToObject(hills, "thunder_timer_offset", g_config.thunder_timer_offset);
    cJSON_AddItemToObject(map_specific, "hills", hills);

    cJSON *dark_tower = cJSON_CreateObject();
    cJSON_AddNumberToObject(dark_tower, "stalactites_timer", g_config.stalactites_timer);
    cJSON_AddNumberToObject(dark_tower, "stalactites_timer_offset", g_config.stalactites_timer_offset);
    cJSON_AddItemToObject(map_specific, "dark_tower", dark_tower);

    cJSON_AddBoolToObject(map_specific, "act9_walls", g_config.act9_walls);
    cJSON_AddBoolToObject(map_specific, "pf_bring_spawn", g_config.pf_bring_spawn);
    cJSON_AddBoolToObject(map_specific, "rmz_easy_mode", g_config.rmz_easy_mode);
    cJSON_AddNumberToObject(map_specific, "ycr_gas_delay", g_config.ycr_gas_delay);
    cJSON_AddBoolToObject(map_specific, "nap_snowballs", g_config.nap_snowballs);
    cJSON_AddNumberToObject(map_specific, "tc_acid_delay", g_config.tc_acid_delay);
    cJSON_AddNumberToObject(map_specific, "hddoor_toggle_delay", g_config.hddoor_toggle_delay);

    cJSON_AddItemToObject(json, "map_specific", map_specific);

	RAssert(collection_save(CONFIG_FILE, json));
    cJSON_Delete(json);
	return true;
}
