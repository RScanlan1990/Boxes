using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class Relay : MonoBehaviour
{
    private static UnityTransport _transport;
    private static UnityTransport Transport
    {
        get => _transport != null ? _transport : _transport = Object.FindObjectOfType<UnityTransport>();
        set => _transport = value;
    }

    async void Awake()
    {
        await Authenticate();
    }

    private static async Task Authenticate()
    {
        //Initialize unity services
        await UnityServices.InitializeAsync();
        //Annomoysuly sign into the services.
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateGame()
    {
        //Create an allocation on the relay service, passing a max of two players.
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

        //Request a join code based on our allocation on the relay service.
        //The join code is used by other players to join our game.
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        //Send the allocation to the transport layer
        Transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

        NetworkManager.Singleton.StartHost();
    }

    public async void JoinGame(string joinCode)
    {
        //Join allocation on the relay service, using the join code.
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        //Send the allocation to the transport layer
        Transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }
}
