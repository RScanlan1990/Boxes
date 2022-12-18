using Boxes.Connection;
using Boxes.Connection.Services;
using Boxes.Connection.Services.Lobbies;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using VContainer;

namespace Boxes.Game
{
    /// <summary>
    /// A mediator class that is responsible for passing UI inputs to their associated classes.
    /// </summary>
    public class LobbyUIMediator : MonoBehaviour
    {
        [SerializeField] Button m_playButton;

        AuthenticationFacade _authenticationServiceFacade;
        LobbyFacade _lobbyFacade;    
        LocalLobbyUser _localUser;
        LocalLobby _localLobby;
        ConnectionManager _connectionManager;

        [Inject]
        void InjectDependenciesAndInitialize(AuthenticationFacade authenticationServiceFacade,
            LobbyFacade lobbyFacade,
            LocalLobbyUser localUser,
            LocalLobby localLobby,
            ConnectionManager connectionManager)
        {
            _authenticationServiceFacade = authenticationServiceFacade;
            _lobbyFacade = lobbyFacade;
            _localUser = localUser;
            _localLobby = localLobby;
            _connectionManager = connectionManager;
        }

        public async void PlayRequestAsync()
        {
            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await _lobbyFacade.TryJoinOrCreateLobbyAsync();


            if (result.Result == LobbyFacade.LobbyRequestResult.Joined)
            {
                OnJoinedLobby(result.Lobby);
            }
            else if (result.Result == LobbyFacade.LobbyRequestResult.Created)
            {
                _localUser.IsHost = true;
                _lobbyFacade.SetRemoteLobby(result.Lobby);

                Debug.Log($"Created lobby with ID: {_localLobby.LobbyID} and code {_localLobby.LobbyCode}");
                _connectionManager.StartHostLobby(_localUser.DisplayName);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        void BlockUIWhileLoadingIsInProgress()
        {
            m_playButton.interactable = false;
            //TODO ADD LOADING SCREEN / SPINNER
           // m_LoadingSpinner.SetActive(true);
        }

        void UnblockUIAfterLoadingIsComplete()
        {
            //this callback can happen after we've already switched to a different scene
            //in that case the canvas group would be null
            if (m_playButton != null)
            {
                m_playButton.interactable = true;
               // m_LoadingSpinner.SetActive(false);
            }
        }

        void OnJoinedLobby(Unity.Services.Lobbies.Models.Lobby remoteLobby)
        {
            _lobbyFacade.SetRemoteLobby(remoteLobby);

            Debug.Log($"Joined lobby with code: {_localLobby.LobbyCode}, Internal Relay Join Code{_localLobby.RelayJoinCode}");
           
            //We have joined a lobby, now connect to external services. 
            _connectionManager.StartClientLobby(_localUser.DisplayName);
        }
    }
}