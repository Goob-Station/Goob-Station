// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 PixelTK <85175107+PixelTheKermit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Eagle <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 nikitosych <boriszyn@gmail.com>
// SPDX-FileCopyrightText: 2025 vanx <61917534+Vaaankas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using System.Numerics;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Add to an entity to paralyze it whenever it reaches critical amounts of Stamina DamageType.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class StaminaComponent : Component
{
    /// <summary>
    /// Have we reached peak stamina damage and been paralyzed?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool Critical;

    /// <summary>
    /// How much stamina reduces per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float Decay = 5f; // goob edit

    /// <summary>
    /// How much time after receiving damage until stamina starts decreasing.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float Cooldown = 5f; // goob edit

    /// <summary>
    /// How much stamina damage this entity has taken.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float StaminaDamage;

    /// <summary>
    /// How much stamina damage is required to entire stam crit.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float CritThreshold = 100f;

    /// <summary>
    /// Goob Edit: A dictionary of active stamina drains, with the key being the source of the drain,
    /// DrainRate how much it changes per tick, and ModifiesSpeed if it should slow down the user.
    /// </summary>
    /// <remarks>
    /// TODO: Refactor into a struct or another component at some point idk.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public Dictionary<string, (float DrainRate, bool ModifiesSpeed, NetEntity? Source, bool ApplyResistances)> ActiveDrains = new();

    /// <summary>
    /// How long will this mob be stunned for?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(6);

    /// <summary>
    /// To avoid continuously updating our data we track the last time we updated so we can extrapolate our current stamina.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public ProtoId<AlertPrototype> StaminaAlert = "Stamina";

    // Goobstation
    [DataField]
    public float StaminaOnShove = 7.5f;

    /// <summary>
    /// This flag indicates whether the value of <see cref="StaminaDamage"/> decreases after the entity exits stamina crit.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AfterCritical;

    /// <summary>
    /// This float determines how fast stamina will regenerate after exiting the stamina crit.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AfterCritDecayMultiplier = 5f;

    /// <summary>
    /// Thresholds that determine an entity's slowdown as a function of stamina damage.
    /// </summary>
    [DataField]
    public Dictionary<FixedPoint2, float> StunModifierThresholds = new() { {0, 1f }, { 60, 0.7f }, { 80, 0.5f } };

    #region Animation Data

    /// <summary>
    /// Threshold at which low stamina animations begin playing. This should be set to a value that means something.
    /// At 50, it is aligned so when you hit 60 stun the entity will be breathing once per second (well above hyperventilation).
    /// </summary>
    [DataField]
    public float AnimationThreshold = 50;

    /// <summary>
    /// Minimum y vector displacement for breathing at AnimationThreshold
    /// </summary>
    [DataField]
    public float BreathingAmplitudeMin = 0.04f;

    /// <summary>
    /// Maximum y vector amount we add to the BreathingAmplitudeMin
    /// </summary>
    [DataField]
    public float BreathingAmplitudeMod = 0.04f;

    /// <summary>
    /// Minimum vector displacement for jittering at AnimationThreshold
    /// </summary>
    [DataField]
    public float JitterAmplitudeMin;

    /// <summary>
    /// Maximum vector amount we add to the JitterAmplitudeMin
    /// </summary>
    [DataField]
    public float JitterAmplitudeMod = 0.04f;

    /// <summary>
    /// Min multipliers for JitterAmplitude in the X and Y directions, animation randomly chooses between these min and max multipliers
    /// </summary>
    [DataField]
    public Vector2 JitterMin = Vector2.Create(0.5f, 0.125f);

    /// <summary>
    /// Max multipliers for JitterAmplitude in the X and Y directions, animation randomly chooses between these min and max multipliers
    /// </summary>
    [DataField]
    public Vector2 JitterMax = Vector2.Create(1f, 0.25f);

    /// <summary>
    /// Minimum total animations per second
    /// </summary>
    [DataField]
    public float FrequencyMin = 0.25f;

    /// <summary>
    /// Maximum amount we add to the Frequency min just before crit
    /// </summary>
    [DataField]
    public float FrequencyMod = 1.75f;

    /// <summary>
    /// Jitter keyframes per animation
    /// </summary>
    [DataField]
    public int Jitters = 4;

    /// <summary>
    /// Vector of the last Jitter so we can make sure we don't jitter in the same quadrant twice in a row.
    /// </summary>
    [DataField]
    public Vector2 LastJitter;

    /// <summary>
    ///     The offset that an entity had before jittering started,
    ///     so that we can reset it properly.
    /// </summary>
    [DataField]
    public Vector2 StartOffset = Vector2.Zero;

    #endregion

    /// <summary>
    /// Goobstation - Used for the sprinting event to get rather we sprinting or not from Goob Mod folder
    /// </summary>
    [DataField]
    public bool IsSprinting { get; set; }
}
