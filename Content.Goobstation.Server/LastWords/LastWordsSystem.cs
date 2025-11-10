using Content.Server.Chat.Systems;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Goobstation.Common.LastWords;

namespace Content.Goobstation.Server.LastWords;

public sealed class LastWordsSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    public override void Initialize()
        => SubscribeLocalEvent<MindContainerComponent, EntitySpokeEvent>(OnEntitySpoke);

    private void OnEntitySpoke(EntityUid uid, MindContainerComponent mindContainerComp, EntitySpokeEvent args)
    {
        _mindSystem.TryGetMind(uid, out var mindId, out _, mindContainerComp);

        if(TryComp<LastWordsComponent>(mindId, out var lastWordsComp))
            lastWordsComp.LastWords = args.Message;
    }
}
