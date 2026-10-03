using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Allows the mech to move faster while it's overclocked.
/// While overclocked it gets increased power drain and loses integrity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MechOverclockComponent : Component
{
    [DataField]
    public EntProtoId ActionProto = "ActionMechOverclock";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public float ActiveSpeedMultiplier = 1.5f;

    [DataField]
    public FixedPoint2 OverclockedPowerDrainPerSecond = 8;

    [DataField]
    public DamageSpecifier DamagePerSecond = new()
    {
        DamageDict = new() { { "Heat", 2 } },
    };

    [DataField]
    public EntProtoId GlowProto = "EffectMechOverclockGlow";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? GlowEntity;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;

    [ViewVariables, AutoNetworkedField]
    public bool Active;
}
