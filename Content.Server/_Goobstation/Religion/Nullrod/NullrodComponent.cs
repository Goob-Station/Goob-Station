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
        public String UntrainedUseString = default!;

    }
}
