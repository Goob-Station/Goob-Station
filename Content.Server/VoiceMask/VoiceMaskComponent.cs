// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Speech;
using Content.Shared.StatusIcon; // Goobstation
using Robust.Shared.Prototypes;

namespace Content.Server.VoiceMask;

/// <summary>
///     This component is for voice mask items! Adding this component to clothing will give the the voice mask UI
///     and allow the wearer to change their voice and verb at will.
/// </summary>
/// <remarks>
///     DO NOT use this if you do not want the interface.
///     The VoiceOverrideSystem is probably what your looking for (Or you might have to make something similar)!
/// </remarks>
[RegisterComponent]
public sealed partial class VoiceMaskComponent : Component
{
    /// <summary>
    ///     The name that will override an entities default name. If null, it will use the default override.
    /// </summary>
    [DataField]
    public string? VoiceMaskName = null;

    /// <summary>
    ///     The speech verb that will override an entities default one. If null, it will use the entities default verb.
    /// </summary>
    [DataField]
    public ProtoId<SpeechVerbPrototype>? VoiceMaskSpeechVerb;

    /// <summary>
    ///     The action that gets displayed when the voice mask is equipped.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionChangeVoiceMask";

    /// <summary>
    ///     Reference to the action.
    /// </summary>
    [DataField]
    public EntityUid? ActionEntity;
    /// <summary>
    ///     if UI Action shud be added on equipt
    /// </summary>
    [DataField]
    public bool EnableAction = true; //Goobstation

    #region GabyStation
    /// <summary>
    ///     The job icon to be displayed next to their name when speaking on radio
    /// </summary>
    [DataField]
    public ProtoId<JobIconPrototype>? JobIconProtoId; // GabyStation -> Radio icons

    /// <summary>
    ///     The name of the job that should show up when a mouse overs over the job icon on the radio
    /// </summary>
    [DataField]
    public string? JobName; // GabyStation -> Radio icons
    #endregion
}
