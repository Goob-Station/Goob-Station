using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._Goobstation.Religion.Nullrod
{
    [RegisterComponent]
    public sealed partial class NullrodComponent : Component
    {
        /// <summary>
        /// How much damage is dealt when an untrained user uses it.
        /// </summary>
        [DataField("DamageOnUntrainedUse", required: true)]
        public DamageSpecifier DamageOnUntrainedUse = default!;

        /// <summary>
        /// Which pop-up string to use.
        /// </summary>
        [DataField("UntrainedUseString", required: true)]
        public string UntrainedUseString = default!;

        /// <summary>
        /// Which sound to play on untrained use.
        /// </summary>
        [DataField]
        public SoundSpecifier UntrainedUseSound = new SoundPathSpecifier("/Audio/Effects/hallelujah.ogg");

    }
}
