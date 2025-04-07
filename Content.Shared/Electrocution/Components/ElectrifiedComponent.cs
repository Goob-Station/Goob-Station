// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ScarKy0 <scarky0@onet.eu>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Electrocution;

/// <summary>
///     Component for things that shock users on touch.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ElectrifiedComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// Should player get damage on collide
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool OnBump = true;

    /// <summary>
    /// Should player get damage on attack
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool OnAttacked = true;

    /// <summary>
    /// When true - disables power if a window is present in the same tile
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool NoWindowInTile = false;

    /// <summary>
    /// Should player get damage on interact with empty hand
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool OnHandInteract = true;

    /// <summary>
    /// Should player get damage on interact while holding an object in their hand
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool OnInteractUsing = true;

    /// <summary>
    /// Indicates if the entity requires power to function
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RequirePower = true;

    /// <summary>
    /// Indicates if the entity uses APC power
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UsesApcPower = false;

    /// <summary>
    /// Identifier for the high voltage node.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? HighVoltageNode;

    /// <summary>
    /// Identifier for the medium voltage node.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? MediumVoltageNode;

    /// <summary>
    /// Identifier for the low voltage node.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? LowVoltageNode;

    /// <summary>
    /// Damage multiplier for HV electrocution
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HighVoltageDamageMultiplier = 3f;

    /// <summary>
    /// Shock time multiplier for HV electrocution
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HighVoltageTimeMultiplier = 1.5f;

    /// <summary>
    /// Damage multiplier for MV electrocution
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MediumVoltageDamageMultiplier = 2f;

    /// <summary>
    /// Shock time multiplier for MV electrocution
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MediumVoltageTimeMultiplier = 1.25f;

    [DataField, AutoNetworkedField]
    public float ShockDamage = 7.5f;

    /// <summary>
    /// Shock time, in seconds.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ShockTime = 8f;

    [DataField, AutoNetworkedField]
    public float SiemensCoefficient = 1f;

    [DataField, AutoNetworkedField]
    public SoundSpecifier ShockNoises = new SoundCollectionSpecifier("sparks");

    [DataField, AutoNetworkedField]
    public SoundPathSpecifier AirlockElectrifyDisabled = new("/Audio/Machines/airlock_electrify_on.ogg");

    [DataField, AutoNetworkedField]
    public SoundPathSpecifier AirlockElectrifyEnabled = new("/Audio/Machines/airlock_electrify_off.ogg");

    [DataField, AutoNetworkedField]
    public bool PlaySoundOnShock = true;

    [DataField, AutoNetworkedField]
    public float ShockVolume = 20;

    [DataField, AutoNetworkedField]
    public float Probability = 1f;

    [DataField, AutoNetworkedField]
    public bool IsWireCut = false;

    /// <summary>
    /// Goobstation
    /// Whether this will ignore target insulation
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreInsulation;

    /// <summary>
    /// Goobstation
    /// Don't shock this entity
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? IgnoredEntity;
}