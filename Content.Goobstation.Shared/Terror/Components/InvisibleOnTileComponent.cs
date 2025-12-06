namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent]
public sealed partial class InvisibleOnTileComponent : Component
{
    [DataField]
    public float Invisibility = 0.25f;

    [DataField]
    public float ExpireTime = 3f;

}
