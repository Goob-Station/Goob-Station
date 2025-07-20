// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Audio;
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
    public bool CanSprint = true;

    [DataField, AutoNetworkedField]
    public float StaminaDrainRate = 15f;

    [DataField, AutoNetworkedField]
    public float SprintSpeedMultiplier = 1.3f;

    [DataField, AutoNetworkedField]
    public TimeSpan TimeBetweenSprints = TimeSpan.FromSeconds(3);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan LastSprint = TimeSpan.Zero;

    [DataField]
    public string StaminaDrainKey = "sprint";

    [DataField]
    public EntProtoId SprintAnimation = "SprintAnimation";

    [ViewVariables]
    public TimeSpan LastStep = TimeSpan.Zero;

    [DataField]
    public EntProtoId StepAnimation = "SmallSprintAnimation";

    [DataField]
    public SoundSpecifier SprintStartupSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/Sprinting/sprint_puff.ogg");

    [DataField, AutoNetworkedField]
    public TimeSpan TimeBetweenSteps = TimeSpan.FromSeconds(0.6);

    [DataField]
    public DamageSpecifier SprintDamageSpecifier = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 6.5 }, // real
        }
    };
}

[Serializable, NetSerializable]
public sealed class SprintToggleEvent(bool isSprinting) : EntityEventArgs
{
    public bool IsSprinting = isSprinting;
}

[Serializable, NetSerializable]
public sealed class SprintStartEvent : EntityEventArgs;

[ByRefEvent]
public sealed class SprintAttemptEvent : CancellableEntityEventArgs;
