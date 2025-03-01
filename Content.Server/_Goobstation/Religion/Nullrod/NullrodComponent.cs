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

        [DataField("selfDamage", required: true)]
        public DamageSpecifier SelfDamage = default!;

        /// <summary>
        /// The pop-up displayed when an untrained user uses it.
        /// </summary>

        [DataField("failPopup", required: true)]
        public string FailPopup = default!;
    }
}
