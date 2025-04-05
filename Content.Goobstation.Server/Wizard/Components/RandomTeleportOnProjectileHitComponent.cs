using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Wizard.Components;

[RegisterComponent]
public sealed partial class RandomTeleportOnProjectileHitComponent : Component
{
    [DataField]
    public EntityWhitelist Whitelist = new();
}
