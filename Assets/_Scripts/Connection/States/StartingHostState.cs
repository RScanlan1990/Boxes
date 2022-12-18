using Boxes.Connection.Methods;
using Boxes.Connection.Services.Lobbies;
using System;
using UnityEngine;
using VContainer;
using Boxes.Connection;

namespace Boxes.Connection.States
{
    /// <summary>
    /// Connection state corresponding to a host starting up. Starts the host when entering the state. If successful,
    /// transitions to the Hosting state, if not, transitions back to the Offline state.
    /// </summary>
    class StartingHostState : OnlineState
    {
        ConnectionMethod _connectionMethod;

        [Inject]
        LocalLobby _localLobby;

        public StartingHostState Configure(ConnectionMethod baseConnectionMethod)
        {
            _connectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
            StartHost();
        }

        public override void Exit() { }

        private async void StartHost()
        {
            try
            {
                await _connectionMethod.SetupHostConnectionAsync();
                Debug.Log($"Created relay allocation with join code {_localLobby.RelayJoinCode}");

                // NGO's StartHost launches everything
                if (!ConnectionManager.NetworkManager.StartHost())
                {
                    OnClientDisconnect(ConnectionManager.NetworkManager.LocalClientId);
                }
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == ConnectionManager.NetworkManager.LocalClientId)
            {
                StartHostFailed();
            }
        }

        void StartHostFailed()
        {
           // m_ConnectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}
