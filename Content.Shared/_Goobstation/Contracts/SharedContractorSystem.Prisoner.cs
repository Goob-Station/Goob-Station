using Content.Shared._Goobstation.Contracts.Components;
using Content.Shared._Goobstation.DumpContainerOnUse;
using Robust.Shared.Containers;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Contracts;

/// <summary>
/// This handles all the contractor prisoner
/// </summary>
public sealed partial class SharedContractorSystem
{
    private void UpdatePrisoner()
    {
        var prisonerQuery = EntityQueryEnumerator<ContractorPrisonerComponent>();
        while (prisonerQuery.MoveNext(out var uid, out var prisonerComponent))
        {
            if (_gameTiming.CurTime < prisonerComponent.TimeLeft || prisonerComponent.TimeLeft == TimeSpan.Zero)
                continue;

            prisonerComponent.TimeLeft = TimeSpan.Zero;
            SpawnReturnPortal(uid, prisonerComponent);
        }
    }

    private void CheckAndSpawnNukeOpsMap()
    {
        var nukeOpsMap = false;

        foreach (var map in _mapSystem.GetAllMapIds())
        {
            if (!_mapSystem.TryGetMap(map, out var mapUid))
                continue;

            if (!TryGetEntityData(GetNetEntity(mapUid.Value), out _, out var metaDataComponent))
                continue;

            Log.Info(metaDataComponent.EntityName);

            if (metaDataComponent.EntityName != "Syndicate Outpost")
                continue;

            nukeOpsMap = true;
            break;
        }

        if (nukeOpsMap)
            return;

        if (_mapLoader.TryLoadMap(new ResPath("/Maps/_Goobstation/Nonstations/nukieplanet.yml"),
                out var nukiemap,
                out _,
                new DeserializationOptions { InitializeMaps = true }))
            _mapSystem.SetPaused(nukiemap.Value.Comp.MapId,
                false);
    }

    private void StartPrisonTimer(EntityUid target, EntityUid portal)
    {
        var prisonerComp = EnsureComp<ContractorPrisonerComponent>(target);
        prisonerComp.TimeLeft = _gameTiming.CurTime + prisonerComp.PrisonerTime;
        prisonerComp.ReturnCoordinates = Transform(portal).Coordinates;
        prisonerComp.Gear = InitializePrisonerContainer(target);
    }

    private EntityUid InitializePrisonerContainer(EntityUid target)
    {
        var containerEntity = Spawn("ContractorPrisonerBox", MapCoordinates.Nullspace);
        var dumpContainerOnUseComponent = EnsureComp<DumpContainerOnUseComponent>(containerEntity);
        var container =
            _containerSystem.EnsureContainer<Container>(containerEntity, dumpContainerOnUseComponent.ContainerId);

        foreach (var item in _inventorySystem.GetHandOrInventoryEntities(target))
        {
            _containerSystem.Insert(item, container, force: true);
        }

        return containerEntity;
    }
}
