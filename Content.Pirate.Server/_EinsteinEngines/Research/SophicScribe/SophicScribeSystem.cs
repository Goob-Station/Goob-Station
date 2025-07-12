using Content.Server.Abilities.Psionics;
using Content.Server.Chat.Systems;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Goobstation.Server.StationEvents.Events;
using Content.Shared.Chat;
using Content.Shared.Interaction;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.Research.SophicScribe;

public sealed partial class SophicScribeSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_glimmerSystem.GlimmerOutput == 0)
            return; // yes, return. Glimmer value is global.

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<SophicScribeComponent>();
        while (query.MoveNext(out var scribe, out var scribeComponent))
        {
            if (curTime < scribeComponent.NextAnnounceTime)
                continue;

            if (!TryComp<IntrinsicRadioTransmitterComponent>(scribe, out var radio))
                continue;

            var message = Loc.GetString("glimmer-report", ("level", _glimmerSystem.GlimmerOutputString));
            var channel = _prototypeManager.Index<RadioChannelPrototype>("Science");
            _radioSystem.SendRadioMessage(scribe, message, channel, scribe);

            scribeComponent.NextAnnounceTime = curTime + scribeComponent.AnnounceInterval;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SophicScribeComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<GlimmerEventEndedEvent>(OnGlimmerEventEnded);
    }

    private void OnInteractHand(EntityUid uid, SophicScribeComponent component, InteractHandEvent args)
    {
        //TODO: the update function should be removed eventually too.
        if (_timing.CurTime < component.StateTime)
            return;

        component.StateTime = _timing.CurTime + component.StateCD;

        _chat.TrySendInGameICMessage(uid, Loc.GetString("glimmer-report", ("level", _glimmerSystem.GlimmerOutputString)), InGameICChatType.Speak, false); 
    }

    private void OnGlimmerEventEnded(GlimmerEventEndedEvent args)
    {
        var query = EntityQueryEnumerator<SophicScribeComponent>();
        while (query.MoveNext(out var scribe, out _))
        {
            if (!TryComp<IntrinsicRadioTransmitterComponent>(scribe, out var radio)) return;

            // mind entities when...
            var speaker = scribe;
            if (TryComp<MindSwappedComponent>(scribe, out var swapped))
            {
                speaker = swapped.OriginalEntity;
            }

            var message = Loc.GetString(args.Message, ("decrease", args.GlimmerBurned), ("level", _glimmerSystem.GlimmerOutputString));
            var channel = _prototypeManager.Index<RadioChannelPrototype>("Science");
            _radioSystem.SendRadioMessage(speaker, message, channel, speaker);
        }
    }
}
