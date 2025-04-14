using Content.Shared.Damage;

namespace Content.Goobstation.Server.OnPray.HealNearOnPray;

[RegisterComponent]
public sealed partial class HealNearOnPrayComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public List<EntityUid> HealedEntities = new();

    [DataField]
    public int Range = 5;
}
