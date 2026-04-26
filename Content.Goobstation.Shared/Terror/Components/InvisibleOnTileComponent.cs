using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// When an entity stepping on this tile passes the component whitelist, apply temporary stealth. Refreshes when stepping on new tiles.
/// </summary>

[RegisterComponent]
public sealed partial class InvisibleOnTileComponent : Component
{
    /// <summary>
    /// How strong the invisibility is.
    /// </summary>
    [DataField]
    public float Invisibility = 0.25f;

    /// <summary>
    /// How long before the invisiblity expires.
    /// </summary>
    [DataField]
    public float ExpireTime = 3f;

    /// <summary>
    /// Whitelist as to what can trigger this effect, self-explanatory.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

}

[RegisterComponent]
public sealed partial class InvisibleOnTileActiveComponent : Component
{
    public TimeSpan? ExpireAt;
}
