using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._Goobstation.Religion.Nullrod
{
    [RegisterComponent]
    public sealed partial class NullrodComponent : Component
    {

        /// <summary>
        /// Audio played.
        /// </summary>
        [DataField("sizzleSound")]
        public SoundSpecifier SizzleSoundPath = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");
        [DataField("healSound")]
        public SoundSpecifier HealSoundPath = new  SoundPathSpecifier("/Audio/Effects/holy.ogg");

        [DataField("selfDamage", required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier SelfDamage = default!;
    }
}
