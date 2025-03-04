using Content.Shared.Damage;

namespace Content.Server._Goobstation.Religion
{
    [RegisterComponent]
    public sealed partial class WeakToHolyComponent : Component
    {
        /// <summary>
        /// The number holy damage is multiplied by.
        /// </summary>

        [DataField("damageMultiplier", required: true)]
        public float DamageMultiplier = 4f;
        // Setting this to an absurd number for testing purposes.

    }
}
