using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Sprinting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SprinterComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool IsSprinting = false;

    [DataField, AutoNetworkedField]
    public float StaminaDrainRate = 15f;

    [DataField, AutoNetworkedField]
    public float SprintSpeedMultiplier = 1.3f;

    [DataField, AutoNetworkedField]
    public TimeSpan TimeBetweenSprints = TimeSpan.FromSeconds(3);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan LastSprint = TimeSpan.Zero;

    [DataField]
    public EntProtoId SprintAnimation = "SprintAnimation";

    [ViewVariables]
    public TimeSpan LastStep = TimeSpan.Zero;

    [DataField]
    public EntProtoId StepAnimation = "SmallSprintAnimation";

    [DataField, AutoNetworkedField]
    public TimeSpan TimeBetweenSteps = TimeSpan.FromSeconds(0.6);

    [DataField]
    public DamageSpecifier SprintDamageSpecifier = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 10 },
        }
    };
}

[Serializable, NetSerializable]
public sealed class SprintToggleEvent : EntityEventArgs
{
    public bool IsSprinting = false;

    public SprintToggleEvent(bool isSprinting)
    {
        IsSprinting = isSprinting;
    }
}
