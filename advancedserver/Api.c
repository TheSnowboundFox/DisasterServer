#include <enet/enet.h>
#include <io/Dir.h>
#include <io/Threads.h>
#include <Server.h>
#include <States.h>
#include <DyList.h>
#include <Config.h>
#include <Moderation.h>

bool disaster_server_lock(Server* server)
{
	RAssert(server);
	MutexLock(server->state_lock);
	return true;
}

bool disaster_server_unlock(Server* server)
{
	RAssert(server);
	MutexUnlock(server->state_lock);
	return true;
}

uint8_t disaster_server_state(Server* server)
{
	RAssert(server);
	return (uint8_t)server->state;
}

bool disaster_server_ban(Server* server, uint16_t id)
{
	for (size_t i = 0; i < server->peers.capacity; i++)
	{
		PeerData* v = (PeerData*)server->peers.ptr[i];
		if (!v)
			continue;

		if (v->id == id)
		{
			server_disconnect(server, v->peer, DR_BANNEDBYHOST, NULL);
			return ban_add(v->nickname.value, v->udid.value, v->ip.value);
		}
	}

	return false;
}

bool disaster_server_op(Server* server, uint16_t id)
{
	for (size_t i = 0; i < server->peers.capacity; i++)
	{
		PeerData* v = (PeerData*)server->peers.ptr[i];
		if (!v)
			continue;

		if (v->id == id)
		{
			v->op = true;

			server_send_msg(server, v->peer, CLRCODE_GRN "you're an operator now");
			return op_add(v->nickname.value, v->ip.value, g_config.moderation.op_default_level, "Default Note...");
		}
	}

	return false;
}

bool disaster_server_timeout(Server* server, uint16_t id, double timeout)
{
	for (size_t i = 0; i < server->peers.capacity; i++)
	{
		PeerData* v = (PeerData*)server->peers.ptr[i];
		if (!v)
			continue;

		if (v->id == id)
		{
			server_disconnect(server, v->peer, DR_KICKEDBYHOST, NULL);
			return timeout_set(v->nickname.value, v->udid.value, v->ip.value, time(NULL) + (uint64_t)(round(timeout)));
		}
	}

	return false;
}

bool disaster_server_peer(Server* server, int index, PeerInfo* info)
{
	info->character = -1;
	if (index < 0 || index >= server->peers.capacity)
		return false;
	else
	{
		PeerData* v = (PeerData*)server->peers.ptr[index];
		if (!v)
			return false;
		else
		{
			info->peer = v;
			info->is_exe = (server->game.exe == v->id);
			info->ip_addr = v->ip;

			if (v->in_game && server->state >= ST_GAME)
			{
				if (info->is_exe)
					info->character = v->exe_char;
				else
					info->character = v->surv_char;
			}
		}
	}

	return true;
}

bool disaster_server_peer_disconnect(Server* server, uint16_t id, DisconnectReason reason, const char* text)
{
	return server_disconnect_id(server, (int)id, reason, text);
}

int disaster_server_peer_count(Server* server)
{
	return server_total(server);
}

int disaster_server_peer_ingame(Server* server)
{
	return server_ingame(server);
}

int8_t disaster_game_map(Server* server)
{
	return server->game.map;
}

double disaster_game_time(Server* server)
{
	return server->game.time;
}

uint16_t disaster_game_time_sec(Server* server)
{
	return server->game.time_sec;
}

Config disaster_get_config(void)
{
	return g_config;
}