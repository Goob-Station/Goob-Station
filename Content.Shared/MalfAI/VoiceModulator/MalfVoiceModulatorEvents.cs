// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.MalfAI;

/// <summary>
/// Server -> Client: Open the Malf voice modulator window with initial configuration.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfVoiceModulatorOpenUiEvent : EntityEventArgs
{
    public readonly bool Enabled;
    public readonly string VoiceName;
    public readonly string? JobProtoId;
    public readonly string? Accent;

    public MalfVoiceModulatorOpenUiEvent(bool enabled, string voiceName, string? jobProtoId, string? accent)
    {
        Enabled = enabled;
        VoiceName = voiceName;
        JobProtoId = jobProtoId;
        Accent = accent;
    }
}

/// <summary>
/// Server -> Client: Push state updates to an open voice modulator window.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfVoiceModulatorStateEvent : EntityEventArgs
{
    public readonly bool Enabled;
    public readonly string VoiceName;
    public readonly string? JobProtoId;
    public readonly string? Accent;

    public MalfVoiceModulatorStateEvent(bool enabled, string voiceName, string? jobProtoId, string? accent)
    {
        Enabled = enabled;
        VoiceName = voiceName;
        JobProtoId = jobProtoId;
        Accent = accent;
    }
}

/// <summary>
/// Client -> Server: Update voice modulator configuration (toggle status, name, job, accent).
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfVoiceModulatorUpdateEvent : EntityEventArgs
{
    public readonly bool Enabled;
    public readonly string VoiceName;
    public readonly string? JobProtoId;
    public readonly string? Accent;

    public MalfVoiceModulatorUpdateEvent(bool enabled, string voiceName, string? jobProtoId, string? accent)
    {
        Enabled = enabled;
        VoiceName = voiceName;
        JobProtoId = jobProtoId;
        Accent = accent;
    }
}
