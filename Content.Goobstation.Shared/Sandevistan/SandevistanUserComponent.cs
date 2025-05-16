using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Sandevistan;

[RegisterComponent]
public sealed partial class SandevistanUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Enabled;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextExecutionTime = TimeSpan.Zero;

    [DataField]
    public string ActionProto = "ActionToggleSandevistan";

    [DataField]
    public EntityUid? ActionUid;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentLoad = 0f;

    [DataField]
    public float LoadPerActiveSecond = 1f;

    [DataField]
    public float LoadPerInactiveSecond = -0.1f;

    [DataField]
    public SortedDictionary<SandevistanState, FixedPoint2> Thresholds = new()
    {
        { SandevistanState.Normal, 0 },
        { SandevistanState.Warning, 10 },
        { SandevistanState.Shaking, 20 },
        { SandevistanState.Pain, 30 },
        { SandevistanState.Damage, 40 },
        { SandevistanState.Stun, 50 },
    };

    [DataField]
    public float Stamina = 5f; // Not used but I'll leave this for yaml warriors

    [DataField]
    public float Pain = 69f; // I have no clue on pain values, will adjust later

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 7.5 },
        },
    };

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(10);

    [DataField]
    public float MovementSpeedModifier = 2f;

    [DataField]
    public float AttackSpeedModifier = 2f;

    [DataField]
    public SoundSpecifier? StartSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_start.ogg");

    [DataField]
    public SoundSpecifier? EndSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_end.ogg");

    [DataField] // So it fits the audio
    public TimeSpan? Delay = TimeSpan.FromSeconds(3);

    [DataField]
    public EntityUid? RunningSound;
}

public sealed partial class ToggleSandevistanEvent : InstantActionEvent;
