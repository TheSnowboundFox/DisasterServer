using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using ExeNet;

namespace DisasterServer.State
{
    internal class CharacterSelect : DisasterServer.State.State
    {
        private Map _map;
        private Dictionary<ushort, Character> _selectedCharacters = new Dictionary<ushort, Character>();
        private Dictionary<ushort, ExeCharacter> _selectedExeCharacters = new Dictionary<ushort, ExeCharacter>();

        public CharacterSelect(Map map) => this._map = map;

        public override DisasterServer.Session.State AsState() => DisasterServer.Session.State.CHARACTERSELECT;

        public override void PeerJoined(Server server, TcpSession session, Peer peer)
        {
        }

        public override void PeerLeft(Server server, TcpSession session, Peer peer)
        {
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            // Handle TCP messages for character selection
            // ...
        }

        public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, ref byte[] data)
        {
        }

        public override void Init(Server server)
        {
            // Initialize character selection phase
            // ...
        }

        public override void Tick(Server server)
        {
            // Check if all players have made their selections
            if (AllPlayersSelected(server))
            {
                // If all players have selected characters, proceed to the next phase
                server.SetState<Game>(new Game(_map, GetExePlayer(server).ID));
            }
        }

        private bool AllPlayersSelected(Server server)
        {
            lock (server.Peers)
            {
                // Check if all non-waiting players have selected characters
                foreach (var peer in server.Peers.Values.Where(p => !p.Waiting))
                {
                    if (!_selectedCharacters.ContainsKey(peer.ID) ||
                        (peer.Player.Character == Character.Exe && !_selectedExeCharacters.ContainsKey(peer.ID)))
                    {
                        // If any player has not selected a character, return false
                        return false;
                    }
                }
            }

            // All players have selected characters
            return true;
        }

        private Peer GetExePlayer(Server server)
        {
            lock (server.Peers)
            {
                // Find the player with the Exe character
                foreach (var peer in server.Peers.Values)
                {
                    if (peer.Player.Character == Character.Exe)
                    {
                        return peer;
                    }
                }
            }

            // If no player has the Exe character, return the first peer
            return server.Peers.Values.FirstOrDefault();
        }
    }
}
