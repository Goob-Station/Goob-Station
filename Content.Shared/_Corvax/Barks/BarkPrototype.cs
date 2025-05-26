// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Zekins <zekins3366@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Corvax.Speech.Synthesis;

/// <summary>
/// A prototype for the available barges.
/// </summary>
[Prototype("bark")]
public sealed class BarkPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// The name of the voice.
    /// </summary>
    [DataField]
    public string Name { get; } = string.Empty;

    /// <summary>
    /// A set of sounds used for speech.
    /// </summary>
    [DataField(required: true)]
    public List<string> SoundFiles { get; } = new();

    /// <summary>
    /// Volume for sounds.
    /// </summary>
    [DataField]
    public float Volume;

    /// <summary>
    /// Whether it is available for selection.
    /// </summary>
    [DataField("roundstart")]
    public bool RoundStart { get; } = true;
}
