using Content.Shared._Goobstation.Contracts.Components;
using Content.Shared.Coordinates;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;

namespace Content.Shared._Goobstation.Contracts;

public sealed partial class  SharedContractorSystem
{
    private void InitializePortal()
    {
        SubscribeLocalEvent<ContractorPortalComponent, PreventCollideEvent>(OnPortalCollide);
    }

    private void UpdatePortal()
    {
        var contractorPortalQuery = EntityQueryEnumerator<ContractorPortalComponent>();
        while (contractorPortalQuery.MoveNext(out var uid, out var contractorPortalComponent))
        {
            if (contractorPortalComponent.Used)
                QueueDel(uid);
        }
    }

    private void CreatePortalAndLink(ContractorUplinkComponent comp)
    {
        var extractPortal = Spawn("PortalRed", GetEntity(comp.FlareUid).ToCoordinates());

        if (!TryComp<PortalComponent>(extractPortal, out var portalComp))
            return;

        if (!TryComp<ContractorComponent>(GetEntity(comp.User), out var contractorComp))
            return;

        portalComp.CanTeleportToOtherMaps = true;
        portalComp.RandomTeleport = false; // prevents fucky shit
        comp.PortalSpawnTimer = TimeSpan.Zero;

        var contractorPortalComponent = EnsureComp<ContractorPortalComponent>(extractPortal);
        contractorPortalComponent.Target = GetEntity(contractorComp.CurrentTarget);

        QueueDel(GetEntity(comp.FlareUid));
        comp.FlareUid = NetEntity.Invalid;

        CheckAndSpawnNukeOpsMap();
        var warpMarker = SelectRandomWarp();
        if (warpMarker != null)
            _linkedEntitySystem.TryLink(extractPortal, warpMarker.Value);
    }

    private EntityUid? SelectRandomWarp()
    {
        // Select a random warp from the list
        var warpList = new List<EntityUid>();
        var warpEnumerator = EntityQueryEnumerator<ContractorWarpMarkerComponent>();

        while (warpEnumerator.MoveNext(out var warpUid, out _))
        {
            warpList.Add(warpUid);
        }

        if (warpList.Count > 0)
            return _random.Pick(warpList);

        Log.Warning("No warp markers found. Returning a null value");
        return null;
    }

    // prevent portal from colliding with the contractor or a non targetted entity
    private void OnPortalCollide(Entity<ContractorPortalComponent> ent, ref PreventCollideEvent args)
    {
        if (args.OtherEntity != ent.Comp.Target)
        {
            args.Cancelled = true;
            return;
        }

        if (args.OtherEntity == ent.Comp.Target)
            args.Cancelled = false;

        StartPrisonTimer(args.OtherEntity, args.OurEntity);
        ent.Comp.Used = true;
    }
    private void SpawnReturnPortal(EntityUid uid, ContractorPrisonerComponent prisonerComponent)
    {
        // Spawn the return portal
        var returnPortal = Spawn("PortalBlue", GetTileInFrontOfEntity(uid));
        var contractorPortalComp = EnsureComp<ContractorPortalComponent>(returnPortal);
        contractorPortalComp.Target = uid;

        if (!TryComp<PortalComponent>(returnPortal, out var portalComp))
            return;
        portalComp.RandomTeleport = false;
        portalComp.CanTeleportToOtherMaps = true;

        var warpMarker = Spawn("ContractorTempMarker", prisonerComponent.ReturnCoordinates);
        _linkedEntitySystem.TryLink(returnPortal, warpMarker);
    }
}
