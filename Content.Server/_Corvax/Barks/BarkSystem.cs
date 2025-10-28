using Content.Shared.Corvax.CorvaxVars;
using Content.Server.Chat.Systems;
using Content.Shared._Corvax.Speech.Synthesis;
using Content.Shared._Corvax.Speech.Synthesis.Components;
using Robust.Shared.Configuration;

namespace Content.Server._Corvax.Speech.Synthesis.System;

public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpeechSynthesisComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnEntitySpoke(EntityUid uid, SpeechSynthesisComponent comp, EntitySpokeEvent args)
    {
        if (comp.VoicePrototypeId is null
            || !args.Language.SpeechOverride.RequireSpeech
            || !_configurationManager.GetCVar(CorvaxVars.BarksEnabled))
            return;

        var sourceEntity = _entityManager.GetNetEntity(uid);
        RaiseNetworkEvent(new PlayBarkEvent(sourceEntity, args.Message, args.IsWhisper));
    }
}
