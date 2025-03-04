using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Goobstation.Religion.Nullrod
{
    [RegisterComponent]
    public sealed partial class AltarSourceComponent : Component
    {

        /// <summary>
        /// Which prototype to create.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("rodProto", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string RodProto = "Nullrod";

        /// <summary>
        /// Which effect to display.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("effectProto", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string EffectProto = "EffectSpark";

        /// <summary>
        /// Which sound effect to play.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("soundPath", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string SoundPath = "/Audio/Effects/hallelujah.ogg";

    }
}
