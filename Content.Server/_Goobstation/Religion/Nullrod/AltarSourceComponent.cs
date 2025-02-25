using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Goobstation.Religion.Nullrod
{
    [RegisterComponent]
    public sealed partial class AltarSourceComponent : Component
    {

        /// <summary>
        /// Which prototype to check for.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("interactProto", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string InteractProto = "Nullrod";

        /// <summary>
        /// Which prototype to create.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("rodProto", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string RodProto = "Nullrod";

    }
}
