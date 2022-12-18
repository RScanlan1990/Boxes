using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boxes.Connection.States
{
    abstract class OnlineState : ConnectionState
    {
        public const string connectionType = "dtls";

        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            // m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }

        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}

