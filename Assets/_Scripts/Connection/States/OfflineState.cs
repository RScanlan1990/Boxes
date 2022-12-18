using Boxes.Connection.Methods;
using Boxes.Connection.Services.Lobbies;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Boxes.Connection.States
{
    class OfflineState : ConnectionState
    {
        [Inject]
        LobbyFacade _lobbyFacade;
        [Inject]
        ProfileManager _profileManager;
        [Inject]
        LocalLobby _localLobby;

        const string k_MainMenuSceneName = "MainMenu";

        public override void Enter()
        {
            //Upon entering the offline state, load the main menu.
            if (SceneManager.GetActiveScene().name != k_MainMenuSceneName)
            {
                SceneLoaderWrapper.Instance.LoadScene(k_MainMenuSceneName, useNetworkSceneManager: false);
            }
        }

        public override void Exit()
        {
            //Upon exiting the offline state, do nothing.
        }

        public override void StartHostLobby(string playerName)
        {
            var connectionMethod = new ConnectionMethodRelay(_lobbyFacade, _localLobby, ConnectionManager, _profileManager, playerName);
            ConnectionManager.ChangeState(ConnectionManager.StartingHostState.Configure(connectionMethod));
        }

        public override void StartClientLobby(string playerName)
        {
            //var connectionMethod = new ConnectionMethodRelay(_lobbyFacade, _localLobby, ConnectionManager, _profileManager, playerName);
            //ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
            //ConnectionManager.ChangeState(ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
        }
    }
}