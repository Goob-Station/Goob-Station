using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.MobPhases;
using Content.Shared.Chat;
using Content.Server.Chat.Systems;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Megafauna.Systems;
public sealed class MegafaunaPhaseSpeechSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MegafaunaPhaseSpeechComponent, MobPhaseChangedEvent>(OnPhaseChanged);
    }
    public override void Update(float frameTime)
    {
        foreach (var comp in EntityQuery<MegafaunaPhaseSpeechComponent>())
        {
            var uid = comp.Owner;

            comp.NextSpeechTime -= frameTime;
            if (comp.NextSpeechTime > 0f)
                continue;

            if (!TrySpeakPhaseLine(uid))
            {
                // No valid speech — try again later
                comp.NextSpeechTime = _random.NextFloat(comp.MinDelay, comp.MaxDelay);
                continue;
            }

            // Successfully spoke — schedule next line
            comp.NextSpeechTime = _random.NextFloat(comp.MinDelay, comp.MaxDelay);
        }
    }


    private void OnPhaseChanged(
        EntityUid uid,
        MegafaunaPhaseSpeechComponent comp,
        MobPhaseChangedEvent args)
    {
        if (!comp.Phases.TryGetValue(args.NewPhase, out var phase))
            return;

        // Reset timer for next voiceline when changing phases.
        comp.NextSpeechTime = _random.NextFloat(comp.MinDelay, comp.MaxDelay);

        if (phase.SpeechOnPhaseChange is not { } loc)
            return;

        _chat.TrySendInGameICMessage(
            uid,
            Loc.GetString(loc),
            InGameICChatType.Speak,
            false);
    }


    public bool TrySpeakPhaseLine(EntityUid uid)
    {
        if (!TryComp<MobPhasesComponent>(uid, out var phases) ||
            !TryComp<MegafaunaPhaseSpeechComponent>(uid, out var speech))
            return false;

        if (!speech.Phases.TryGetValue(phases.CurrentPhase, out var phase))
            return false;

        if (phase.Speech.Count == 0)
            return false;

        var loc = _random.Pick(phase.Speech);

        _chat.TrySendInGameICMessage(
            uid,
            Loc.GetString(loc),
            InGameICChatType.Speak,
            false);

        return true;
    }

}
