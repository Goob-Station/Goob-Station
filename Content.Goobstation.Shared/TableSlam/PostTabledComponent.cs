namespace Content.Goobstation.Shared.TableSlam;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PostTabledComponent : Component
{
    [DataField]
    public TimeSpan PostTabledShovableTime = TimeSpan.Zero;

    [DataField]
    public float ParalyzeChance = 0.35f;
}
