using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

[RegisterComponent, NetworkedComponent]
public sealed partial class TentacleHookProjectileComponent : Component
{
    [DataField]
    public TimeSpan DurationSlow = TimeSpan.FromSeconds(10);

    [ViewVariables]
    public EntityUid? Target;
}
