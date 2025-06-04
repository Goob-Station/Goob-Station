// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Abilities;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

// Ideally speaking this should be on the heart itself... but this also works.
namespace Content.Goobstation.Shared.Sandevistan;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SandevistanUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Enabled;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan? DisableAt;

    [DataField]
    public TimeSpan StatusEffectTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan NextExecutionTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan PopupDelay = TimeSpan.FromSeconds(3);

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public TimeSpan NextPopupTime = TimeSpan.Zero;

    [DataField]
    public string ActionProto = "ActionToggleSandevistan";

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? ActionUid;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float CurrentLoad = 0f;

    [DataField]
    public float LoadPerActiveSecond = 1f;

    [DataField]
    public float LoadPerInactiveSecond = -0.25f;

    [DataField]
    public SortedDictionary<SandevistanState, FixedPoint2> Thresholds = new()
    {
        { SandevistanState.Normal, 0 },
        { SandevistanState.Warning, 15 },
        { SandevistanState.Shaking, 30 },
        { SandevistanState.Damage, 45 },
        { SandevistanState.Disable, 60 },
    };

    [DataField]
    public float StaminaDamage = 5f;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 5 },
        },
    };

    [DataField]
    public float MovementSpeedModifier = 2f;

    [DataField]
    public float AttackSpeedModifier = 2f;

    [DataField]
    public SoundSpecifier? StartSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_start.ogg");

    [DataField]
    public SoundSpecifier? EndSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_end.ogg");

    [DataField] // So it fits the audio
    public TimeSpan ShiftDelay = TimeSpan.FromSeconds(1.9);

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? RunningSound;

    [ViewVariables(VVAccess.ReadOnly)]
    public DogVisionComponent? DogVision;

    [ViewVariables(VVAccess.ReadOnly)]
    public TrailComponent? Trail;

    [ViewVariables(VVAccess.ReadWrite)]
    public int ColorAccumulator = 0;
}

public sealed partial class ToggleSandevistanEvent : InstantActionEvent;
