// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
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
    /// <summary>
    ///     Is the entity currently sprinting?
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool IsSprinting = false;

    /// <summary>
    ///     Does the entity scale their stamina drain with their stamina modifiers?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ScaleWithStamina = true;

    /// <summary>
    ///     Can the entity sprint?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanSprint = true;

    /// <summary>
    ///     How much stamina is drained per second?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StaminaDrainRate = 9f;

    /// <summary>
    ///     By how much do we multiply stamina recovery while sprinting?
    /// </summary>
    /// <remarks>
    ///     This is used to compensate for the average stamina modifying chem giving you speed.
    ///     Could be made a CVAR but durk takes ages to update those so eh.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public float StaminaRegenMultiplier = 0.75f;

    /// <summary>
    ///     How much do we multiply stamina drains while theres a StaminaModifierComponent?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StaminaDrainMultiplier = 1.4f;

    /// <summary>
    ///     How much do we multiply sprint speed?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SprintSpeedMultiplier = 1.45f;

    /// <summary>
    ///     How long do we have to wait between sprints?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TimeBetweenSprints = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     When did we last sprint?
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public TimeSpan LastSprint = TimeSpan.Zero;

    /// <summary>
    ///     What string do we use to tag stamina drain?
    /// </summary>
    [DataField]
    public string StaminaDrainKey = "sprint";

    /// <summary>
    ///     What entity do we use for sprinting visuals?
    /// </summary>
    [DataField]
    public EntProtoId SprintAnimation = "SprintAnimation";

    /// <summary>
    ///     When did we last step?
    /// </summary>
    [ViewVariables]
    public TimeSpan LastStep = TimeSpan.Zero;

    /// <summary>
    ///     What entity do we use for stepping visuals?
    /// </summary>
    [DataField]
    public EntProtoId StepAnimation = "SmallSprintAnimation";

    /// <summary>
    ///     What sound do we play when we start sprinting?
    /// </summary>
    [DataField]
    public SoundSpecifier SprintStartupSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/Sprinting/sprint_puff.ogg");

    /// <summary>
    ///     How long do we have to wait between spawning step visuals?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TimeBetweenSteps = TimeSpan.FromSeconds(0.6);

    /// <summary>
    ///     What damage specifier do we use if sprinting stops abruptly?
    /// </summary>
    [DataField]
    public DamageSpecifier SprintDamageSpecifier = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 10 },
        }
    };

    /// <summary>
    ///     For how long does entity get knocked down on collision with another sprinting entity?
    /// </summary>
    [DataField]
    public TimeSpan KnockdownDurationOnInterrupt = TimeSpan.FromSeconds(2f);

    /// <summary>
    ///     How much extra stamina damage entity takes for being broken out of sprint with a shove?
    /// </summary>
    [DataField]
    public float StaminaPenaltyOnShove = 25f;
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
