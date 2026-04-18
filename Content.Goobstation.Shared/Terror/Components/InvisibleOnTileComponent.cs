using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// When an entity stepping on this tile passes the component whitelist, apply temporary stealth. Refreshes when stepping on new tiles.
/// </summary>

[RegisterComponent]
public sealed partial class InvisibleOnTileComponent : Component
{
    [DataField]
    public float Invisibility = 0.25f;

    [DataField]
    public float ExpireTime = 3f;

    [DataField]
    public TimeSpan? ExpireAt;

    [DataField]
    public EntityWhitelist? Whitelist;

}
