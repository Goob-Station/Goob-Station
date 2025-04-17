// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.TapeRecorder;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedTapeRecorderSystem))]
[AutoGenerateComponentState]
public sealed partial class TapeCassetteComponent : Component
{
    /// <summary>
    /// A list of all recorded voice, containing timestamp, name and spoken words
    /// </summary>
    [DataField]
    public List<TapeCassetteRecordedMessage> RecordedData = new();

    /// <summary>
    /// The current position within the tape we are at, in seconds
    /// Only dirtied when the tape recorder is stopped
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CurrentPosition = 0f;

    /// <summary>
    /// Maximum capacity of this tape
    /// </summary>
    [DataField]
    public TimeSpan MaxCapacity = TimeSpan.FromSeconds(120);

    /// <summary>
    /// How long to spool the tape after it was damaged
    /// </summary>
    [DataField]
    public TimeSpan RepairDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// When an entry is damaged, the chance of each character being corrupted.
    /// </summary>
    [DataField]
    public float CorruptionChance = 0.25f;

    /// <summary>
    /// Temporary storage for all heard messages that need processing
    /// </summary>
    [DataField]
    public List<TapeCassetteRecordedMessage> Buffer = new();

    /// <summary>
    /// Whitelist for tools that can be used to respool a damaged tape.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist RepairWhitelist = new();
}
