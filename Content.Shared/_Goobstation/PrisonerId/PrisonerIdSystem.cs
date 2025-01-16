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
    [Dependency] protected readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] protected readonly SharedAccessSystem _accessSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PrisonerIdComponent,ComponentStartup>(OnStartup);
        //SubscribeNetworkEvent<SpawnedPrisonerId>(OnSpawned);
        SubscribeNetworkEvent<StartPrisonerSentence>(StartTimer);

    }

    private void OnSpawned(SpawnedPrisonerId args)
    {

    }

    private void OnStartup(EntityUid uid, PrisonerIdComponent component, ComponentStartup args)
    {
        //throw new NotImplementedException();
    }

    private IEnumerable<ProtoId<AccessLevelPrototype>> ConvertListHashSetToEnumerable(List<HashSet<ProtoId<AccessLevelPrototype>>> listOfHashSets)
    {
        return listOfHashSets.SelectMany(hashSet => hashSet);
    }

    public void StartTimer(StartPrisonerSentence args)
    {
        Logger.Debug("Network event fired");
        var uid = GetEntity(args.Uid);
        if (!TryComp<PrisonerIdComponent>(uid, out var comp))
            return;
        // miliseconds
        var time = (int) comp.SentenceTime * 1000;

        Timer.Spawn(time,
            () =>
            {
                Logger.Debug("should be freed?");
                _accessSystem.TrySetTags(uid, ConvertListHashSetToEnumerable(comp.AccessLists));
            });



    }
}
