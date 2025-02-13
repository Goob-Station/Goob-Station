namespace Content.Shared._White.Collision.Blur;

[RegisterComponent]
public sealed partial class BlurOnCollideComponent : Component
{
    [DataField]
    public TimeSpan BlurTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan BlindTime = TimeSpan.Zero;
}
