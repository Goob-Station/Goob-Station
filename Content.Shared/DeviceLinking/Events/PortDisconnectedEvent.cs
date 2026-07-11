// SPDX-License-Identifier: MIT

namespace Content.Shared.DeviceLinking.Events
{
    public sealed class PortDisconnectedEvent : EntityEventArgs
    {
        public readonly string Port;
        public readonly EntityUid RemovedPortUid; // Einstein Engines

        public PortDisconnectedEvent(string port, EntityUid removedPortUid) // Einstein Engines - added Uid
        {
            Port = port;
            RemovedPortUid = removedPortUid;
        }
    }
}