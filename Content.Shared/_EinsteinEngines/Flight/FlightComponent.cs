// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._EinsteinEngines.Flight;

/// <summary>
///     Adds an action that allows the user to become temporarily
///     weightless at the cost of stamina and hand usage.
/// </summary>
[RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
public sealed partial class FlightComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? ToggleAction = "ActionToggleFlight";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    /// <summary>
    ///     Is the user flying right now?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool On;

    /// <summary>
    ///     Stamina drain per second when flying
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StaminaDrainRate = 13.0f;

    /// <summary>
    ///     By how much do we multiply stamina recovery while flying?
    /// </summary>
    /// <remarks>
    ///     This is used to compensate for our reduction of stamina drains below.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public float StaminaRegenMultiplier = 0.25f;

    /// <summary>
    ///     How much do we multiply stamina drains while theres a StaminaModifierComponent?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StaminaDrainMultiplier = 0.85f;

    /// <summary>
    ///     String key to identify the stamina drain within the dictionary.
    /// </summary>
    [DataField]
    public string StaminaDrainKey = "flight";

    /// <summary>
    ///     DoAfter delay until the user becomes weightless.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ActivationDelay = 1.0f;

    /// <summary>
    ///     How much does this modify the weightless acceleration and speed?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpeedModifier = 1.8f;

    /// <summary>
    ///     How much does this modify the weightless friction?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FrictionModifier = 5f;

    /// <summary>
    ///     How much does this modify the weightless friction when no input is applied?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FrictionNoInputModifier = 25f;

    /// <summary>
    ///     Path to a sound specifier or collection for the noises made during flight
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier FlapSound = new SoundCollectionSpecifier("WingFlaps");

    /// <summary>
    ///     Is the flight animated?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsAnimated = true;

    /// <summary>
    ///     Does the animation animate a layer?.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsLayerAnimated;

    /// <summary>
    ///     Which RSI layer path does this animate?
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Layer;

    /// <summary>
    ///     Whats the speed of the shader?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ShaderSpeed = 6.0f;

    /// <summary>
    ///     How much are the values in the shader's calculations multiplied by?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ShaderMultiplier = 0.01f;

    /// <summary>
    ///     What is the offset on the shader?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ShaderOffset = 0.25f;

    /// <summary>
    ///     What animation does the flight use?
    /// </summary>
    [DataField, AutoNetworkedField]
    public string AnimationKey = "default";

    /// <summary>
    ///     Does the flight use a different animation when sprinting?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UseSprintAnimation = false;

    /// <summary>
    ///     Time between sounds being played
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FlapInterval = 1.0f;

    public float TimeUntilFlap;

    /// <summary>
    ///     Does this flight behavior change collision masks?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ChangeCollisionMasks = true;

    /// <summary>
    ///     List of fixtures that had their collision mask changed.
    ///     Required for re-adding the collision mask.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(string key, int originalMask)> ChangedFixtures = new();

    /// <summary>
    ///     Does the flight fail (deal damage) when it stops abruptly?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanFail = true;

    /// <summary>
    ///     What damage specifier is used when the flight fails? Smaller than sprinting since its a lot easier to fuck this one up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier FailDamageSpecifier = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 3.5 },
        }
    };
}
