using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Other;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnCollideComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage;

    [DataField]
    public EntityWhitelist? IgnoreWhitelist = new();

    [DataField]
    public EntityWhitelist? Whitelist = new();
}
