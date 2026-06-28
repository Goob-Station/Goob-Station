using Content.Shared._Lavaland.MobPhases;
using Content.Shared.Chat;
using Content.Server.Chat.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed class MobPhaseSpeechSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobPhaseSpeechComponent, MobPhaseChangedEvent>(OnPhaseChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MobPhaseSpeechComponent, MobPhasesComponent>();
        while (query.MoveNext(out var uid, out var speech, out var phases))
        {
            if (_timing.CurTime < speech.NextSpeechTime)
            {
                continue;
            }

            TrySpeakPhaseLine((uid, speech, phases));

            // Whether or not a line was found, push the next attempt out by a random delay.
            speech.NextSpeechTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(speech.MinDelay, speech.MaxDelay));
        }
    }

    private void OnPhaseChanged(Entity<MobPhaseSpeechComponent> ent, ref MobPhaseChangedEvent args)
    {
        if (!ent.Comp.Phases.TryGetValue(args.NewPhase, out var phase))
            return;

        // Reset timer for next voiceline when changing phases.
        ent.Comp.NextSpeechTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.MinDelay, ent.Comp.MaxDelay));

        if (phase.SpeechOnPhaseChange is not { } loc)
            return;

        Speak(ent.Owner, loc);
    }

    /// <summary>
    /// Attempts to speak a random line for the entity's current phase.
    /// </summary>
    public bool TrySpeakPhaseLine(EntityUid uid)
    {
        return TrySpeakPhaseLine((uid, null, null));
    }

    private bool TrySpeakPhaseLine(Entity<MobPhaseSpeechComponent?, MobPhasesComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp1, ref ent.Comp2, false))
            return false;

        if (!ent.Comp1.Phases.TryGetValue(ent.Comp2.CurrentPhase, out var phase))
            return false;

        if (phase.Speech.Count == 0)
            return false;

        Speak(ent.Owner, _random.Pick(phase.Speech));
        return true;
    }

    private void Speak(EntityUid uid, LocId loc)
    {
        _chat.TrySendInGameICMessage(
            uid,
            Loc.GetString(loc),
            InGameICChatType.Speak,
            false);
    }
}
