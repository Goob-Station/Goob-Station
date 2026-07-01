// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat.Prototypes;

namespace Content.Server._Pirate.Speech;

public sealed class PirateAudibleEmoteSystem : EntitySystem
{
    private static readonly HashSet<string> AudibleGeneralEmotes =
    [
        "Blink",
        "BlinkRapid",
    ];

    public bool IsAudible(EmotePrototype emote)
    {
        return emote.Category is EmoteCategory.Vocal or EmoteCategory.Farts ||
               AudibleGeneralEmotes.Contains(emote.ID);
    }
}
