// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Server.NTR
{
    [RegisterComponent]
    public sealed partial class EventTriggerComponent : Component
    {
        [DataField("eventId", required: true)]
        public string EventId = string.Empty;
    }
}
