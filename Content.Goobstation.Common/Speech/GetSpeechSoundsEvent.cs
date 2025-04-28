// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Speech;

[ByRefEvent]
public record struct GetSpeechSoundEvent(string? SpeechSoundProtoId = null);
