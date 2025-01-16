using Content.Shared.StationRecords;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This handles...
/// </summary>
public sealed class PrisonerIdSystem : EntitySystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PrisonerIdComponent,ComponentStartup>(OnStartup);
        SubscribeNetworkEvent<SpawnedPrisonerId>(OnSpawned);
        SubscribeLocalEvent<PrisonerIdComponent,StartPrisonerSentence>(StartTimer);

    }

    private void OnSpawned(SpawnedPrisonerId args)
    {
        var key = new StationRecordKey(args.Key, GetEntity(args.Station));

        if (!TryComp<StationRecordKeyStorageComponent>(GetEntity(args.Uid), out var stationRecordKeyStorageComponent))
            return;

        if (!TryComp<PrisonerIdComponent>(GetEntity(args.Uid), out var prisonerIdComponent))
            return;

        stationRecordKeyStorageComponent.Key = key;
        prisonerIdComponent.SentenceTime = args.Time;
        Dirty(GetEntity(args.Uid),stationRecordKeyStorageComponent);
        Dirty(GetEntity(args.Uid),prisonerIdComponent);
    }

    private void OnStartup(EntityUid uid, PrisonerIdComponent component, ComponentStartup args)
    {
        //throw new NotImplementedException();
    }

    public void StartTimer(EntityUid uid, PrisonerIdComponent component, StartPrisonerSentence args)
    {
        throw new NotImplementedException();
    }
}
