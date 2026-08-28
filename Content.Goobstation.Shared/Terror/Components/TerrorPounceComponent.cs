using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Pounce towards location, if hits living target, stun, knock down and inject them with nasty black tar
/// if hits a wall, stuns the pouncer instead.
/// </summary>
[RegisterComponent]
public sealed partial class TerrorPounceComponent : Component
{
    [DataField]
    public float JumpDistance = 3f;

    [DataField]
    public float JumpThrowSpeed = 5f;

    [DataField]
    public SoundSpecifier? JumpSound = new SoundPathSpecifier("/Audio/_Goobstation/Terror/Effects/terror_pounce.ogg");

    [DataField]
    public TimeSpan TargetStun = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan TargetKnockdown = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan SelfStun = TimeSpan.FromSeconds(1);

    [DataField]
    public ProtoId<ReagentPrototype>? InjectReagent;

    [DataField]
    public FixedPoint2 InjectAmount = FixedPoint2.New(5);

    [DataField]
    public bool IsLeaping;
}
