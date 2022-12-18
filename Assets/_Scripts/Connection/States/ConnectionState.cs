using Unity.Netcode;
using VContainer;

namespace Boxes.Connection.States
{
    abstract class ConnectionState
    {
        [Inject]
        protected ConnectionManager ConnectionManager;

        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnClientConnection(ulong clientId) { }
        public virtual void OnClientDisconnect(ulong clientId) { }
        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }
        public virtual void OnUserRequestedShutdown() { }
        public virtual void OnTransportFailure() { }
        public virtual void StartHostLobby(string playerName) { }
        public virtual void StartClientLobby(string playerName) { }
    }
}