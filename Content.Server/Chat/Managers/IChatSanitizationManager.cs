// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Chat.Managers;

public interface IChatSanitizationManager
{
    public void Initialize();

    public bool TrySanitizeEmoteShorthands(string input,
        EntityUid speaker,
        out string sanitized,
        [NotNullWhen(true)] out string? emote);
}