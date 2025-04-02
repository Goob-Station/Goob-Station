using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DragonKungFuTimerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastMoveTime = TimeSpan.Zero;

    [DataField]
    public float MinVelocitySquared = 0.25f;

    [DataField]
    public TimeSpan PauseDuration = TimeSpan.FromSeconds(1f);

    [DataField]
    public TimeSpan BuffLength = TimeSpan.FromSeconds(5f);
}
