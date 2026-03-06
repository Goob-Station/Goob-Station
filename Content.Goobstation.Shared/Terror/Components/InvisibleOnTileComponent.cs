namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component used on the tile YAML. Not very generic so currently only makes Grey terrors invisible, but easily changeable.
/// </summary>

[RegisterComponent]
public sealed partial class InvisibleOnTileComponent : Component
{
    [DataField]
    public float Invisibility = 0.25f;

    [DataField]
    public float ExpireTime = 3f;

}
