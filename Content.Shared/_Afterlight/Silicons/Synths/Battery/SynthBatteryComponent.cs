using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Afterlight.Silicons.Synths.Battery;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class SynthBatteryComponent : Component
{
    [DataField]
    public float DrawRate = 0.4f;

    [DataField]
    public float NutrimentChargeMultiplier = 2.4f;

    [DataField]
    public LocId? BatteryLowText;

    [DataField]
    public LocId? BatteryDeadText;

    [DataField]
    public LocId? BatteryEmpText = "synth-battery-emp";

    [DataField]
    public SoundSpecifier? BatteryLowSound;

    [DataField]
    public SoundSpecifier? BatteryDeadSound;

    [DataField]
    public List<float> WarningPercentages = new() { 20f, 10f };

    [DataField]
    public float UnpoweredWalkSpeedModifier = 0.5f;

    [DataField]
    public float UnpoweredSprintSpeedModifier = 0.5f;

    [DataField]
    public DamageSpecifier? EmpDamage;

    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "SynthBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "SynthBatteryNone";

    [DataField]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public bool Unpowered;

    [ViewVariables]
    public TimeSpan NextUpdate;
}
