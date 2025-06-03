// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Abilities;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.Audio;

// Ideally speaking this should be on the heart itself... but this also works.
namespace Content.Goobstation.Shared.Sandevistan;

[RegisterComponent]
public sealed partial class SandevistanUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Enabled;

    [DataField]
    public TimeSpan StatusEffectTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextExecutionTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan PopupDelay = TimeSpan.FromSeconds(3);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextPopupTime = TimeSpan.Zero;

    [DataField]
    public string ActionProto = "ActionToggleSandevistan";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionUid;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentLoad = 0f;

    [DataField]
    public float LoadPerActiveSecond = 1f;

    [DataField]
    public float LoadPerInactiveSecond = -0.15f;

    [DataField]
    public SortedDictionary<SandevistanState, FixedPoint2> Thresholds = new()
    {
        { SandevistanState.Normal, 0 },
        { SandevistanState.Warning, 10 },
        { SandevistanState.Shaking, 20 },
        { SandevistanState.Stamina, 30 },
        { SandevistanState.Damage, 40 },
        { SandevistanState.Knockdown, 50 },
        { SandevistanState.Disable, 60 },
    };

    [DataField]
    public float StaminaDamage = 5f;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 6.5 },
        },
    };

    [ViewVariables(VVAccess.ReadWrite)]
    public float ColorAccumulator = 0f;

    [DataField]
    public float MovementSpeedModifier = 2f;

    [DataField]
    public float AttackSpeedModifier = 2f;

    [DataField]
    public SoundSpecifier? StartSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_start.ogg");

    [DataField]
    public SoundSpecifier? EndSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_end.ogg");

    [DataField] // So it fits the audio
    public TimeSpan ShiftDelay = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan? DisableAt;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? RunningSound;

    [ViewVariables(VVAccess.ReadOnly)]
    public TrailComponent? Trail;

    [ViewVariables(VVAccess.ReadOnly)]
    public DogVisionComponent? DogVision;
}

public sealed partial class ToggleSandevistanEvent : InstantActionEvent;
