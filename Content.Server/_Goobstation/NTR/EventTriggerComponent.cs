using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Goobstation.NTR
{
    [RegisterComponent]
    public sealed partial class EventTriggerComponent : Component
    {
        [DataField("eventId", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string EventId = string.Empty;
    }
}
