namespace Content.Shared._White.Collision.Blur;

[RegisterComponent]
public sealed partial class BlurOnCollideComponent : Component
{
    [DataField]
    public float BlurTime = 5f;
}
