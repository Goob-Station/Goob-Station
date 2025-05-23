// SPDX-FileCopyrightText: 2025 Zekins <zekins3366@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Corvax.Speech.Synthesis;

[Serializable, NetSerializable]
public sealed class RequestPreviewBarkEvent(string barkVoiceId) : EntityEventArgs
{
    public string BarkVoiceId { get; } = barkVoiceId;
}

[Serializable, NetSerializable]
public sealed class PlayBarkEvent : EntityEventArgs
{
    public string SoundPath { get; }
    public NetEntity SourceUid { get; }
    public string Message { get; }
    public float PlaybackSpeed { get; }
    public bool Obfuscated { get; }

    public PlayBarkEvent(string soundPath, NetEntity sourceUid, string message, float playbackSpeed, bool obfuscated)
    {
        SoundPath = soundPath;
        SourceUid = sourceUid;
        Message = message;
        PlaybackSpeed = playbackSpeed;
        Obfuscated = obfuscated;
    }
}
