// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.PDA;

namespace Content.Server.PDA.Ringer;

/// <summary>
/// Opens the store ui when the ringstone is set to the secret code.
/// Traitors are told the code when greeted.
/// </summary>
[RegisterComponent, Access(typeof(RingerSystem))]
public sealed partial class RingerUplinkComponent : Component
{
    /// <summary>
    /// Notes to set ringtone to in order to lock or unlock the uplink.
    /// Automatically initialized to random notes.
    /// </summary>
    [DataField("code")]
    public Note[] Code = new Note[RingerSystem.RingtoneLength];

    /// <summary>
    /// Whether to show the toggle uplink button in pda settings.
    /// </summary>
    [DataField("unlocked"), ViewVariables(VVAccess.ReadWrite)]
    public bool Unlocked;
}