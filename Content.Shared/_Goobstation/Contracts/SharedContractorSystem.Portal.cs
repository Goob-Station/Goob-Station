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
        if(!_net.IsServer)
            return;

        var contractorPortalQuery = EntityQueryEnumerator<ContractorPortalComponent>();
        while (contractorPortalQuery.MoveNext(out var uid, out var contractorPortalComponent))
        {
            if(contractorPortalComponent.TimeToDestroyAfterUse == TimeSpan.Zero || _gameTiming.CurTime < contractorPortalComponent.TimeToDestroyAfterUse)
                continue;

            contractorPortalComponent.TimeToDestroyAfterUse = TimeSpan.Zero;
            QueueDel(uid);
        }
    }

    private void CreatePortalAndLink(ContractorUplinkComponent comp)
    {
        var flareCoords = Transform(GetEntity(comp.FlareUid)).Coordinates;
        QueueDel(GetEntity(comp.FlareUid));
        comp.FlareUid = NetEntity.Invalid;

        var extractPortal = Spawn("PortalRed", flareCoords);

        if (!TryComp<PortalComponent>(extractPortal, out var portalComp))
            return;

        if (!TryComp<ContractorComponent>(GetEntity(comp.User), out var contractorComp))
            return;

        portalComp.CanTeleportToOtherMaps = true;
        portalComp.RandomTeleport = false; // prevents fucky shit
        comp.PortalSpawnTimer = TimeSpan.Zero;

        var contractorPortalComponent = EnsureComp<ContractorPortalComponent>(extractPortal);
        contractorPortalComponent.Target = GetEntity(contractorComp.CurrentTarget);

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
        if(!_net.IsServer)
            return;

        if (args.OtherEntity != ent.Comp.Target)
        {
            args.Cancelled = true;
            return;
        }

        args.Cancelled = false;
        if(ent.Comp.PrisonPortal)
            StartPrisonTimer(args.OtherEntity, args.OurEntity);
        else
        {
            if(TryComp<ContractorPrisonerComponent>(args.OtherEntity, out var prisonerComp))
                _handsSystem.TryForcePickupAnyHand(args.OtherEntity, prisonerComp.Gear);
        }

        ent.Comp.TimeToDestroyAfterUse = _gameTiming.CurTime + ent.Comp.LifetimeAfterUse;
    }

    private void SpawnReturnPortal(EntityUid uid, ContractorPrisonerComponent prisonerComponent)
    {
        // Spawn the return portal
        var returnPortal = Spawn("PortalBlue", GetTileInFrontOfEntity(uid));
        var contractorPortalComp = EnsureComp<ContractorPortalComponent>(returnPortal);
        contractorPortalComp.Target = uid;
        contractorPortalComp.PrisonPortal = false;
        contractorPortalComp.TimeToDestroy = _gameTiming.CurTime + contractorPortalComp.Lifetime;

        if (!TryComp<PortalComponent>(returnPortal, out var portalComp))
            return;
        portalComp.RandomTeleport = false;
        portalComp.CanTeleportToOtherMaps = true;

        var warpMarker = Spawn("ContractorTempMarker", prisonerComponent.ReturnCoordinates);
        _linkedEntitySystem.TryLink(returnPortal, warpMarker);
    }
}
