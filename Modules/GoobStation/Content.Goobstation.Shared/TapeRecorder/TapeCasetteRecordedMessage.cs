// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Speech;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.TapeRecorder;

/// <summary>
/// Every chat event recorded on a tape is saved in this format
/// </summary>
[DataDefinition]
public sealed partial class TapeCassetteRecordedMessage : IComparable<TapeCassetteRecordedMessage>
{
    /// <summary>
    /// Number of seconds since the start of the tape that this event was recorded at
    /// </summary>
    [DataField(required: true)]
    public float Timestamp = 0;

    /// <summary>
    /// The name of the entity that spoke
    /// </summary>
    [DataField]
    public string? Name;

    /// <summary>
    /// The verb used for this message.
    /// </summary>
    [DataField]
    public ProtoId<SpeechVerbPrototype>? Verb;

    /// <summary>
    /// What was spoken
    /// </summary>
    [DataField]
    public string Message = string.Empty;

    public TapeCassetteRecordedMessage(float timestamp, string name, ProtoId<SpeechVerbPrototype> verb, string message)
    {
        Timestamp = timestamp;
        Name = name;
        Verb = verb;
        Message = message;
    }

    public int CompareTo(TapeCassetteRecordedMessage? other)
    {
        if (other == null)
            return 0;

        return (int) (Timestamp - other.Timestamp);
    }
}
