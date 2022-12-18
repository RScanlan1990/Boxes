using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : SingletonNetwork<GameManager>
{
    public NetworkVariable<ulong> PlayerOne;
    public NetworkVariable<ulong> PlayerTwo;
    public NetworkVariable<bool> GameStarted;

    public static NetworkVariable<int> PlayersConnected;

    private void Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback += PlayerLeft;
            PlayersConnected = new NetworkVariable<int> { Value = 0 };
        }
    }

    public void PlayerJoined(ulong clientId)
    {        
        if (PlayersConnected.Value == 0) 
        {
            PlayerOne.Value = clientId;
        } 
        else if(PlayersConnected.Value == 1)
        {
            PlayerTwo.Value = clientId;
        }

        PlayersConnected.Value++;

        if (PlayersConnected.Value == 1)
        {
            GameStarted.Value = true;
        }
    }

    public void PlayerLeft(ulong clientId)
    {

    }
}
