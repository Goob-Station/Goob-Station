// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI.VoiceModulator;

/// <summary>
/// Sent from server to client to open the voice modulator UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfVoiceModulatorOpenUiEvent : EntityEventArgs;

/// <summary>
/// Sent from client to server with the chosen voice name and optional speech verb.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfVoiceModulatorSubmitNameEvent : EntityEventArgs
{
    public string Name;

    /// <summary>
    /// SpeechVerbPrototype id to use when speaking, or null to keep the current verb.
    /// </summary>
    public string? SpeechVerb;

    /// <summary>
    /// JobIconPrototype id displayed next to the name on radio, or null to keep the AI icon.
    /// </summary>
    public string? JobIcon;

    public MalfVoiceModulatorSubmitNameEvent(string name, string? speechVerb = null, string? jobIcon = null)
    {
        Name = name;
        SpeechVerb = speechVerb;
        JobIcon = jobIcon;
    }
}
