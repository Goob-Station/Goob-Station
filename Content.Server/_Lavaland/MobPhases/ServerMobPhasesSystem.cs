using Content.Server.Chat.Systems;
using Content.Shared._Lavaland.MobPhases;
using Content.Shared.Chat;

namespace Content.Server._Lavaland.MobPhases;

public sealed partial class ServerMobPhasesSystem : MobPhasesSystem
{
    [Dependency] private ChatSystem _chat = default!;

    [SubscribeLocalEvent]
    private void OnPhaseChanged(Entity<MobPhasesComponent> ent, ref MobPhaseChangedEvent args)
    {
        if (args.NewPhase > args.OldPhase
            && TryGetPhase(ent.Comp, args.NewPhase, out var next)
            && next.SpeechOnEnter is { } loc)
            _chat.TrySendInGameICMessage(ent, Loc.GetString(loc), InGameICChatType.Speak, false);
    }
}
