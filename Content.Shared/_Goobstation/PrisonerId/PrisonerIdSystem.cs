using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This handles...
/// </summary>
public sealed class PrisonerIdSystem : EntitySystem
{
    [Dependency] private readonly SharedAccessSystem _accessSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeNetworkEvent<StartPrisonerSentence>(StartTimer);

    }

    private IEnumerable<ProtoId<AccessLevelPrototype>> ConvertListHashSetToEnumerable(List<HashSet<ProtoId<AccessLevelPrototype>>> listOfHashSets)
    {
        return listOfHashSets.SelectMany(hashSet => hashSet);
    }

    public void StartTimer(StartPrisonerSentence args)
    {
        var uid = GetEntity(args.Uid);
        if (!TryComp<PrisonerIdComponent>(uid, out var comp))
            return;
        var time = (int) comp.SentenceTime * 1000;

        Timer.Spawn(time,
            () =>
            {
                _accessSystem.TrySetTags(uid, ConvertListHashSetToEnumerable(comp.AccessLists));
            });
    }
}
