using JetBrains.Annotations;

namespace Content.Goobstation.Server.Insanity;

[RegisterComponent]
public sealed partial class InsanityComponent : Component
{
    [DataField]
    public TimeSpan NextInsanityTick = TimeSpan.Zero;

    [DataField]
    public TimeSpan ExecutionInterval = TimeSpan.FromSeconds(15);

    [DataField]
    public bool IsBlinded;

    [DataField]
    public bool IsMuted;
}
