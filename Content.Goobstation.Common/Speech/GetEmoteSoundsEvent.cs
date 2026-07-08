// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Speech;

[ByRefEvent]
public record struct GetEmoteSoundsEvent(string? EmoteSoundProtoId = null, bool Handled = false);
