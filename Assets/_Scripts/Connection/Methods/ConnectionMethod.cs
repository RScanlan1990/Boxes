using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using Boxes.Connection.Services.Lobbies;
using Boxes.Connection.States;

namespace Boxes.Connection.Methods
{
    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
        public string playerName;
        public bool isDebug;
    }

    public abstract class ConnectionMethod
    {
        protected ConnectionManager ConnectionManager;
        protected readonly string PlayerName;

        private ProfileManager _profileManager;

        public abstract Task SetupHostConnectionAsync();

        public abstract Task SetupClientConnectionAsync();

        public ConnectionMethod(ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
        {
            ConnectionManager = connectionManager;
            _profileManager = profileManager;
            PlayerName = playerName;
        }

        protected void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });

            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        protected string GetPlayerId()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
               // return ClientPrefs.GetGuid() + m_ProfileManager.Profile;
            }

            //TODO Use client prefs + profile manager to generate name if cannot get player id.
            //return AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : ClientPrefs.GetGuid() + m_ProfileManager.Profile;
            return AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : "UNKNOWN PLAYER";
        }
    }

    class ConnectionMethodRelay : ConnectionMethod
    {
        LobbyFacade _lobbyFacade;
        LocalLobby _localLobby;

        public ConnectionMethodRelay(LobbyFacade lobbyServiceFacade, LocalLobby localLobby, ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
            : base(connectionManager, profileManager, playerName)
        {
            _lobbyFacade = lobbyServiceFacade;
            _localLobby = localLobby;
            ConnectionManager = connectionManager;
        }

        public override async Task SetupClientConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay client");

            SetConnectionPayload(GetPlayerId(), PlayerName);

            if (_lobbyFacade.CurrentUnityLobby == null)
            {
                throw new Exception("Trying to start relay while Lobby isn't set");
            }

            Debug.Log($"Setting Unity Relay client with join code {_localLobby.RelayJoinCode}");

            // Create client joining allocation from join code
            var joinedAllocation = await RelayService.Instance.JoinAllocationAsync(_localLobby.RelayJoinCode);
            Debug.Log($"client: {joinedAllocation.ConnectionData[0]} {joinedAllocation.ConnectionData[1]}, " +
                $"host: {joinedAllocation.HostConnectionData[0]} {joinedAllocation.HostConnectionData[1]}, " +
                $"client: {joinedAllocation.AllocationId}");

            await _lobbyFacade.UpdatePlayerRelayInfoAsync(joinedAllocation.AllocationId.ToString(), _localLobby.RelayJoinCode);

            // Configure UTP with allocation
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(joinedAllocation, OnlineState.connectionType));
        }

        public override async Task SetupHostConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay host");

            SetConnectionPayload(GetPlayerId(), PlayerName); // Need to set connection payload for host as well, as host is a client too

            // Create relay allocation
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(ConnectionManager.MaxConnectedPlayers, region: null);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            Debug.Log($"server: connection data: {hostAllocation.ConnectionData[0]} {hostAllocation.ConnectionData[1]}, " +
                $"allocation ID:{hostAllocation.AllocationId}, region:{hostAllocation.Region}");

            _localLobby.RelayJoinCode = joinCode;

            //next line enable lobby and relay services integration
            await _lobbyFacade.UpdateLobbyDataAsync(_localLobby.GetDataForUnityServices());
            await _lobbyFacade.UpdatePlayerRelayInfoAsync(hostAllocation.AllocationIdBytes.ToString(), joinCode);

            // Setup UTP with relay connection info
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(hostAllocation, OnlineState.connectionType)); // This is with DTLS enabled for a secure connection
        }
    }

    public class ProfileManager
    {

    }
}
