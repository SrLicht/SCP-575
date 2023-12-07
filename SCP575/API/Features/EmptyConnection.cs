using Mirror;
using System;

namespace SCP575.API.Features
{
    /// <summary>
    /// Empty connectio for NPC to use it.
    /// </summary>
    public class EmptyConnection : NetworkConnectionToClient
    {
        /// <inheritdoc/>
        public override string address => "localhost";

        /// <summary>
        /// Create a new instance of <see cref="EmptyConnection"/>
        /// </summary>
        /// <param name="connectionId"></param>
        public EmptyConnection(int connectionId) : base(connectionId)
        {
        }

        /// <inheritdoc/>
        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }
    }
}
