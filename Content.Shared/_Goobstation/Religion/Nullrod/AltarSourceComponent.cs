using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.Religion.Nullrod
{
    [RegisterComponent]
    public sealed partial class AltarSourceComponent : Component
    {

        /// <summary>
        /// Which prototype to create.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("rodProto")]
        public EntProtoId RodProto = "Nullrod";

        /// <summary>
        /// Which effect to display.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("effectProto")]
        public EntProtoId EffectProto = "EffectSpark";

        /// <summary>
        /// Which sound effect to play.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("soundPath")]
        public SoundSpecifier? SoundPath;

    }
}
