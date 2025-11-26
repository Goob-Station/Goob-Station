using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mobs;

namespace Content.Goobstation.Server.Chemistry.InjectOnMobState;

[RegisterComponent]
public sealed partial class InjectOnMobStateComponent : Component
{
    [DataField]
    public MobState State = MobState.Critical;

    [DataField]
    public Dictionary<string, FixedPoint2> Reagents = new();

    [DataField]
    public float Cooldown = 180;

    public TimeSpan NextUse = TimeSpan.Zero;
}
