using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Boxes.Connection.Services.Lobbies
{
    public struct LobbyData
    {
        public string LobbyID { get; set; }
        public string LobbyCode { get; set; }
        public string RelayJoinCode { get; set; }
        public string LobbyName { get; set; }
        public bool Private { get; set; }
        public int MaxPlayerCount { get; set; }

        public LobbyData(LobbyData existing)
        {
            LobbyID = existing.LobbyID;
            LobbyCode = existing.LobbyCode;
            RelayJoinCode = existing.RelayJoinCode;
            LobbyName = existing.LobbyName;
            Private = existing.Private;
            MaxPlayerCount = existing.MaxPlayerCount;
        }

        public LobbyData(string lobbyCode)
        {
            LobbyID = null;
            LobbyCode = lobbyCode;
            RelayJoinCode = null;
            LobbyName = null;
            Private = false;
            MaxPlayerCount = -1;
        }
    }

    public sealed class LocalLobby
    {
        Dictionary<string, LocalLobbyUser> m_LobbyUsers = new Dictionary<string, LocalLobbyUser>();
        LobbyData m_Data;

        public Dictionary<string, LocalLobbyUser> LobbyUsers => m_LobbyUsers;
        public LobbyData Data => new LobbyData(m_Data);

        public event Action<LocalLobby> changed;

        public Dictionary<string, DataObject> GetDataForUnityServices() =>
        new Dictionary<string, DataObject>()
        {
                            {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public,  RelayJoinCode)}
        };

        public string LobbyCode
        {
            get => m_Data.LobbyCode;
            set
            {
                m_Data.LobbyCode = value;
                OnChanged();
            }
        }

        public string RelayJoinCode
        {
            get => m_Data.RelayJoinCode;
            set
            {
                m_Data.RelayJoinCode = value;
                OnChanged();
            }
        }

        public string LobbyID
        {
            get => m_Data.LobbyID;
            set
            {
                m_Data.LobbyID = value;
                OnChanged();
            }
        }

        public void ApplyRemoteData(Lobby lobby)
        {
            var info = new LobbyData(); // Technically, this is largely redundant after the first assignment, but it won't do any harm to assign it again.
            info.LobbyID = lobby.Id;
            info.LobbyCode = lobby.LobbyCode;
            info.Private = lobby.IsPrivate;
            info.LobbyName = lobby.Name;
            info.MaxPlayerCount = lobby.MaxPlayers;

            if (lobby.Data != null)
            {
                info.RelayJoinCode = lobby.Data.ContainsKey("RelayJoinCode") ? lobby.Data["RelayJoinCode"].Value : null; // By providing RelayCode through the lobby data with Member visibility, we ensure a client is connected to the lobby before they could attempt a relay connection, preventing timing issues between them.
            }
            else
            {
                info.RelayJoinCode = null;
            }

            var lobbyUsers = new Dictionary<string, LocalLobbyUser>();
            foreach (var player in lobby.Players)
            {
                if (player.Data != null)
                {
                    if (LobbyUsers.ContainsKey(player.Id))
                    {
                        lobbyUsers.Add(player.Id, LobbyUsers[player.Id]);
                        continue;
                    }
                }

                // If the player isn't connected to Relay, get the most recent data that the lobby knows.
                // (If we haven't seen this player yet, a new local representation of the player will have already been added by the LocalLobby.)
                var incomingData = new LocalLobbyUser
                {
                    IsHost = lobby.HostId.Equals(player.Id),
                    DisplayName = player.Data?.ContainsKey("DisplayName") == true ? player.Data["DisplayName"].Value : default,
                    ID = player.Id
                };

                lobbyUsers.Add(incomingData.ID, incomingData);
            }

            CopyDataFrom(info, lobbyUsers);
        }

        public void CopyDataFrom(LobbyData data, Dictionary<string, LocalLobbyUser> currUsers)
        {
            m_Data = data;

            if (currUsers == null)
            {
                m_LobbyUsers = new Dictionary<string, LocalLobbyUser>();
            }
            else
            {
                List<LocalLobbyUser> toRemove = new List<LocalLobbyUser>();
                foreach (var oldUser in m_LobbyUsers)
                {
                    if (currUsers.ContainsKey(oldUser.Key))
                    {
                        oldUser.Value.CopyDataFrom(currUsers[oldUser.Key]);
                    }
                    else
                    {
                        toRemove.Add(oldUser.Value);
                    }
                }

                foreach (var remove in toRemove)
                {
                    DoRemoveUser(remove);
                }

                foreach (var currUser in currUsers)
                {
                    if (!m_LobbyUsers.ContainsKey(currUser.Key))
                    {
                        DoAddUser(currUser.Value);
                    }
                }
            }

            OnChanged();
        }

        void DoAddUser(LocalLobbyUser user)
        {
            m_LobbyUsers.Add(user.ID, user);
            user.changed += OnChangedUser;
        }

        void DoRemoveUser(LocalLobbyUser user)
        {
            if (!m_LobbyUsers.ContainsKey(user.ID))
            {
                Debug.LogWarning($"Player {user.DisplayName}({user.ID}) does not exist in lobby: {LobbyID}");
                return;
            }

            m_LobbyUsers.Remove(user.ID);
            user.changed -= OnChangedUser;
        }

        void OnChangedUser(LocalLobbyUser user)
        {
            OnChanged();
        }

        void OnChanged()
        {
            changed?.Invoke(this);
        }


    }
}

