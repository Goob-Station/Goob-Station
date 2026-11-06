using Content.Shared.EntityEffects;
using Content.Goobstation.Shared.EntityEffects;
using Content.Shared.Speech;
using Content.Server.Chat.Systems;
using Robust.Shared.Prototypes;
using Content.Shared.Chat;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class SayEntityEffectSystem : EntityEffectSystem<SpeechComponent, SayEffect>
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IRobustRandom _random = default!;

    protected override void Effect(Entity<SpeechComponent> ent, ref EntityEffectEvent<SayEffect> args)
    {
        string? str = null;

        if (_proto.TryIndex(args.Effect.DatasetId, out var dataset))
        {
            str = _random.Pick(dataset.Values);
        }
        else if (args.Effect.LocId is { } locId)
        {
            str = locId;
        }

        if (str is null)
            return;

        _chat.TrySendInGameICMessage(ent, Loc.GetString(str), InGameICChatType.Speak, false);
    }
}
