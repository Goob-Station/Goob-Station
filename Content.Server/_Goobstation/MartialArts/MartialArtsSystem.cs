using Content.Server.Chat.Systems;
using Content.Shared._Goobstation.MartialArts;
using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared._Goobstation.MartialArts.Events;

namespace Content.Server._Goobstation.MartialArts;

public sealed class MartialArtsSystem : SharedMartialArtsSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CanPerformComboComponent, SleepingCarpSaying>(OnSleepingCarpSaying);
    }

    private void OnSleepingCarpSaying(Entity<CanPerformComboComponent> ent, ref SleepingCarpSaying args)
    {
        _chat.TrySendInGameICMessage(ent, args.Saying, InGameICChatType.Speak, false);
    }
}
