using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Curses;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseDeathComponent : Component
{
    [DataField]
    public EntProtoId SmokeProto = "AdminInstantEffectSmoke3";

    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public ProtoId<ReagentPrototype> Reagent = "Ammonia";

    [DataField]
    public float SmokeDuration = 7f;

    [DataField]
    public int SmokeSpread = 18;

    [DataField]
    public FixedPoint2 WpGeneration = 2;
}
