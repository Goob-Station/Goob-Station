using Content.Shared.Whitelist;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class IceCubeOnProjectileHitComponent : Component
{
    [DataField]
    public EntityWhitelist Whitelist = new();
}
