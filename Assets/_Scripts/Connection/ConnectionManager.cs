using Boxes.Connection.States;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Boxes.Connection
{
    public class ConnectionManager : MonoBehaviour
    {
        [Inject]
        NetworkManager m_networkManager;
        public NetworkManager NetworkManager => m_networkManager;

        ConnectionState _currentConnectionState;


        internal readonly OfflineState OfflineState = new OfflineState();
        internal readonly StartingHostState StartingHostState = new StartingHostState();

        public int MaxConnectedPlayers = 2;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            _currentConnectionState = OfflineState;

            //NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            //NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            //NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            //NetworkManager.OnTransportFailure += OnTransportFailure;
        }

        void OnDestroy()
        {
            //NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            //NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            //NetworkManager.OnServerStarted -= OnServerStarted;
            NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            //NetworkManager.OnTransportFailure -= OnTransportFailure;
        }

        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            _currentConnectionState.ApprovalCheck(request, response);
        }

        internal void ChangeState(ConnectionState nextState)
        {
            Debug.Log($"{name}: Changed connection state from {_currentConnectionState.GetType().Name} to {nextState.GetType().Name}.");

            if (_currentConnectionState != null)
            {
                _currentConnectionState.Exit();
            }
            _currentConnectionState = nextState;
            _currentConnectionState.Enter();
        }

        public void StartClientLobby(string playerName)
        {
            _currentConnectionState.StartClientLobby(playerName);
        }

        public void StartHostLobby(string playerName)
        {
            _currentConnectionState.StartHostLobby(playerName);
        }

        ////Called on every client/server when this object is spawned.
        //public override void OnNetworkSpawn()
        //{
        //    //Only bind the callback if this being called on the server.
        //    if (IsServer)
        //    {
        //        NetworkManager.NetworkConfig.ConnectionApproval = true;
        //        NetworkManager.ConnectionApprovalCallback = ConnectionApproval;
        //    }
        //}

        ////Called on the server whenever a player joins the server
        //private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request,
        //    NetworkManager.ConnectionApprovalResponse response)
        //{
        //    // The client identifier to be authenticated
        //    var clientId = request.ClientNetworkId;

        //    // Additional connection data defined by user code
        //    // We can use this to pass data from the client to the server when the client is connecting.
        //    var connectionData = request.Payload;

        //    // Your approval logic determines the following values
        //    response.Approved = true;
        //    response.CreatePlayerObject = true;

        //    // The prefab hash value of the NetworkPrefab, if null the default NetworkManager player prefab is used

        //    var playerPrefab = _numberOfPlayerConnected == 1 ? PlayerPrefabs[0] : PlayerPrefabs[1];
        //    response.PlayerPrefabHash = playerPrefab;

        //    // Position to spawn the player object
        //    var startPosition = _numberOfPlayerConnected == 1 ? PlayerOneStartPosition.position : PlayerTwoStartPosition.position;
        //    response.Position = startPosition;

        //    // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        //    response.Rotation = Quaternion.identity;

        //    // If additional approval steps are needed, set this to true until the additional steps are complete
        //    // once it transitions from true to false the connection approval response will be processed.
        //    response.Pending = false;

        //    //var prefab = GameManager.PlayersConnected.Value == 0 ? PlayerOnePrefab : PlayerTwoPrefab;
        //    //response.PlayerPrefabHash = prefab;
        //}
    }
}