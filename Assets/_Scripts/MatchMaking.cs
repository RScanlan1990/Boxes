using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using System;
using Unity.Netcode;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.SceneManagement;

public class MatchMaking : MonoBehaviour
{
    private Lobby _connectedLobby;
    private QueryResponse _lobbies;
    private UnityTransport _transport;
    private const string JoinCodeKey = "j";
    private string _playerId;

    private void Awake() => _transport = FindObjectOfType<UnityTransport>();

    public async void CreateOrJoinLobby()
    {
        await Authenticate();

        //Create or Join a lobby
        _connectedLobby = await QuickJoinLobbyAsync() ?? await CreateLobbyAsync();
        
        //Lock the lobby
        await Lobbies.Instance.UpdateLobbyAsync(_connectedLobby.Id, new UpdateLobbyOptions { IsLocked = true });

        //Load the game
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    private async Task Authenticate()
    {
        //Initialize unity services
        await UnityServices.InitializeAsync();
        //Annomoysuly sign into the services.
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
    }

    private async Task<Lobby> QuickJoinLobbyAsync()
    {
        try
        {
            //Attempt to join a lobby.
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            //If we found one, grab the allocation details.
            var allocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            //Send allocation details to the transport layer.
            _transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            //Start the game.
            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch(Exception e)
        {
            return null;
        }
    }

    private async Task<Lobby> CreateLobbyAsync()
    {
        try
        {
            const int maxPlayers = 2;

            //Create a relay allication and generate a join code.
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            //Create a lobby, adding the join code.
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        JoinCodeKey,
                        new DataObject(DataObject.VisibilityOptions.Public, joinCode)
                    }
                }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxPlayers, options);

            //Lobby will shutdown after 30 seconds of inactivity, so we need to send a heartbeat every 15 seconds to keep it alive.
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            //Send allocation details to the transport layer.
            _transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            return lobby;
        }
        catch (Exception e) 
        {
            Debug.Log($"Failed to create lobby, exception : {e.Message}");
            return null;
        }
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId,
        float waitTime)
    {
        var delay = new WaitForSecondsRealtime(waitTime);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines();

            if(_connectedLobby != null)
            {
                if(_connectedLobby.HostId == _playerId)
                {
                    Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                }
                else
                {
                    Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
                }
            }
        } 
        catch
        {
            Debug.Log("FAILED TO SHIT DOWN LOBBY");
        }
    }
}
