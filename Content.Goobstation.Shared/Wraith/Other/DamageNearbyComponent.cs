using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Other;

/// <summary>
/// Damages entities that are near to the source
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class DamageNearbyComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    [DataField]
    public EntityWhitelist? Whitelist = new();

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public TimeSpan NextTick;

    [DataField]
    public float Range = 5f;
}
