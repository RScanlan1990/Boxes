using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using VContainer.Unity;
using VContainer;
using Unity.Services.Relay;

namespace Boxes.Connection.Services.Lobbies
{
    public class LobbyFacade
    {
        [Inject] LocalLobby _localLobby;
        [Inject] LocalLobbyUser m_LocalUser;

        public Lobby CurrentUnityLobby { get; private set; }

        public enum LobbyRequestResult
        {
            Joined,
            Created,
            Failed
        }

        /// <summary>
        /// Attempt to join the first lobby among the available lobbies that match the filtered onlineMode.
        /// </summary>
        public async Task<(LobbyRequestResult Result, Lobby Lobby)> TryJoinOrCreateLobbyAsync()
        {

            //TODO SET UP RATE LIMITS TO STOP DOS ATTACKS
            //if (!m_RateLimitQuickJoin.CanCall)
            //{
            //    Debug.LogWarning("Quick Join Lobby hit the rate limit.");
            //    return (false, null);
            //}

            try
            {
                //TODO SET UP LOCAL USER DATA FROM A CLIENT DRIVEN SOURCE
                var localUserData = new Dictionary<string, PlayerDataObject>()
                {
                    {"DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Test Player")},
                };

                var joinRequest = new QuickJoinLobbyOptions
                {
                    Filter = new List<QueryFilter>()
                    {
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "0")
                    },
                    Player = new Player(id: AuthenticationService.Instance.PlayerId, data: localUserData)
                };

                var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinRequest);

                return (LobbyRequestResult.Joined, lobby);
            }
            catch (LobbyServiceException e)
            {

                return await TryCreateLobbyAsync("test-lobby", 2, false);
            }
        }

        /// <summary>
        /// Attempt to create a new lobby and then join it.
        /// </summary>
        public async Task<(LobbyRequestResult result, Lobby Lobby)> TryCreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate)
        {
            //TODO DOS ATTACK
            //if (!m_RateLimitHost.CanCall)
            //{
            //    Debug.LogWarning("Create Lobby hit the rate limit.");
            //    return (false, null);
            //}

            try
            {
                CreateLobbyOptions createOptions = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Player(id: AuthenticationService.Instance.PlayerId, data: m_LocalUser.GetDataForUnityServices()),
                    Data = null
                };

                var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
                return (LobbyRequestResult.Created, lobby);
            }
            catch (LobbyServiceException e)
            {
                //TODO HANDLE ERROR
                //if (e.Reason == LobbyExceptionReason.RateLimited)
                //{
                //    m_RateLimitHost.PutOnCooldown();
                //}
                //else
                //{
                //    PublishError(e);
                //}
            }

            return (LobbyRequestResult.Failed, null);
        }

        public void SetRemoteLobby(Lobby lobby)
        {
            CurrentUnityLobby = lobby;
            _localLobby.ApplyRemoteData(lobby);
        }

        /// <summary>
        /// Lobby can be provided info about Relay (or any other remote allocation) so it can add automatic disconnect handling.
        /// </summary>
        public async Task UpdatePlayerRelayInfoAsync(string allocationId, string connectionInfo)
        {
            //TODO DOS PROTECTION
            //if (!m_RateLimitQuery.CanCall)
            //{
            //    return;
            //}

            try
            {
                UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>(),
                    AllocationId = allocationId,
                    ConnectionInfo = connectionInfo
                };

                await LobbyService.Instance.UpdatePlayerAsync(CurrentUnityLobby.Id, AuthenticationService.Instance.PlayerId, updateOptions);
            }
            catch (LobbyServiceException e)
            {
                //TODO ERROR HANDLING
                //if (e.Reason == LobbyExceptionReason.RateLimited)
                //{
                //    m_RateLimitQuery.PutOnCooldown();
                //}
                //else
                //{
                //    PublishError(e);
                //}

                //todo - retry logic? SDK is supposed to handle this eventually
            }
        }


        /// <summary>
        /// Attempt to update a set of key-value pairs associated with a given lobby.
        /// </summary>
        public async Task UpdateLobbyDataAsync(Dictionary<string, DataObject> data)
        {
            //if (!m_RateLimitQuery.CanCall)
            //{
            //    return;
            //}

            var dataCurr = CurrentUnityLobby.Data ?? new Dictionary<string, DataObject>();

            foreach (var dataNew in data)
            {
                if (dataCurr.ContainsKey(dataNew.Key))
                {
                    dataCurr[dataNew.Key] = dataNew.Value;
                }
                else
                {
                    dataCurr.Add(dataNew.Key, dataNew.Value);
                }
            }
        }
    }
}
