using Content.Shared.Damage;

namespace Content.Shared.Abilities.Psionics
{
    /// <summary>
    /// Takes damage when dispelled.
    /// </summary>
    [RegisterComponent]
    public sealed partial class DamageOnDispelComponent : Component
    {
        [DataField(required: true)]
        public DamageSpecifier Damage = default!;

        [DataField]
        public float Variance = 0.5f;
    }
}
