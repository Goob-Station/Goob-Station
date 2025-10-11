using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Shitcode.Heretic.EntitySystems;

public sealed class EnchantedBookSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    private static readonly EntProtoId BookProto = "BookRandomCrewmember";

    private static readonly EntProtoId SpecialBookProto = "BookRandomCrewmemberSpecial";

    // This doesn't work on decapped people. Uhh... skill issue?
    public void BookEm(HashSet<EntityUid> gibs,
        string name,
        MapCoordinates pos,
        bool isSec,
        bool isCommand,
        bool isHeretic)
    {
        var mindContainerQuery = GetEntityQuery<MindContainerComponent>();
        var brain = gibs.FirstOrNull(x => mindContainerQuery.HasComp(x));

        // No book :(
        if (brain == null)
            return;

        var book = Spawn(SelectBookType(), pos);

        var mindContainer = mindContainerQuery.Comp(brain.Value);

        if (TryComp(mindContainer.Mind, out MindComponent? mind))
            _mind.TransferTo(mindContainer.Mind.Value, book, mind: mind);

        _meta.SetEntityName(book, mind?.CharacterName ?? name);

        // Goodbye
        QueueDel(brain);

        return;

        EntProtoId SelectBookType()
        {
            return isSec || isCommand || isHeretic ? SpecialBookProto : BookProto;
        }
    }
}
