using System;
using GameCore;
using Mirror;
using PluginAPI.Core;

namespace SCP575.Resources
{
    public class FakeConnection : NetworkConnectionToClient
    {

        public FakeConnection(int networkConnectionId) : base(networkConnectionId)
        {
        }

        public override string address => "YourMom";

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
            // Ignore
        }

        public override void Disconnect()
        {
            PluginAPI.Core.Log.Debug("Destroying Dummy", Scp575.Instance.Config.Debug);

            try
            {
                Dummies.DestroyDummy(identity.gameObject.GetComponent<ReferenceHub>());
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}