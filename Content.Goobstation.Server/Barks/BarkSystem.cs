using Content.Goobstation.Common.Barks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.Speech;
using Content.Server.Chat.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Barks;

public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeechSynthesisComponent, EntitySpokeEvent>(OnEntitySpoke);
        Subs.CVar(_cfg, GoobCVars.BarksEnabled, x => _enabled = x, true);
    }

    private void OnEntitySpoke(EntityUid uid, SpeechSynthesisComponent comp, EntitySpokeEvent args)
    {
        var ev = new GetBarkSourceEntityEvent();
        RaiseLocalEvent(uid, ref ev);
        PlayBark(ev.Ent ?? uid, args);
    }

    private void PlayBark(Entity<SpeechSynthesisComponent?> ent, EntitySpokeEvent args)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.VoicePrototypeId is null
            || !args.Language.SpeechOverride.RequireSpeech
            || !_enabled)
            return;

        var sourceEntity = GetNetEntity(ent);
        RaiseNetworkEvent(new PlayBarkEvent(sourceEntity, args.Message, args.IsWhisper), Filter.Pvs(ent));
    }
}
