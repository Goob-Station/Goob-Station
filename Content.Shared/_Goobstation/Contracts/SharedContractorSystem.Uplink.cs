using System.Linq;
using Content.Shared._Goobstation.Contracts.Components;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Hands;

namespace Content.Shared._Goobstation.Contracts;

/// <summary>
/// This handles contracts for syndicate contractors.
/// </summary>
public sealed partial class SharedContractorSystem
{

    private void InitializeUplink()
    {
        SubscribeLocalEvent<ContractorUplinkComponent, ContractorUiMessage>(OnUiButtonPressed);
        SubscribeLocalEvent<ContractorUplinkComponent, GotEquippedHandEvent>(OnUplinkEquipped);
        SubscribeLocalEvent<ContractorUplinkComponent, ExtractionDoAfterEvent>(OnExtractionDoAfter);
    }

    private void UpdateUplink()
    {
        var uplinkQuery = EntityQueryEnumerator<ContractorUplinkComponent>();
        while (uplinkQuery.MoveNext(out var uid, out var comp))
        {
            /*
            if (_gameTiming.CurTime > comp.ExtractionCooldown || comp.ExtractionCooldown != TimeSpan.Zero)
            {
                comp.ExtractionCooldown = TimeSpan.Zero;

                if(comp.User != NetEntity.Invalid)
                    UpdateUi(GetEntity(comp.User));
            }*/

            if (_gameTiming.CurTime < comp.PortalSpawnTimer || comp.PortalSpawnTimer == TimeSpan.Zero)
                continue;
            CreatePortalAndLink(comp);
        }
    }

    private void OpenUserInterface(EntityUid user, EntityUid uplink)
    {
        if (!_userInterfaceSystem.HasUi(uplink, ContractorUplinkUiKey.Key))
            return;

        _userInterfaceSystem.OpenUi(uplink, ContractorUplinkUiKey.Key, user);
    }

    private void UpdateUi(EntityUid uid)
    {
        if (!_net.IsServer)
            return;

        if (!_handsSystem.TryGetActiveItem(uid, out var item) ||
            !_userInterfaceSystem.HasUi(item.Value, ContractorUplinkUiKey.Key) ||
            !TryComp<ContractorComponent>(uid, out var contractorComponent)
            || !TryComp<ContractorUplinkComponent>(item.Value, out var contractorUplinkComponent))
            return;


        var state = new ContractorUplinkBoundUserInterfaceState(contractorComponent.Tc,
            contractorComponent.Contracts,
            contractorComponent.Rep,
            contractorComponent.CurrentTarget,
            contractorComponent.CurrentExtractionPoint,
            contractorUplinkComponent.ExtractionCooldown);

        _userInterfaceSystem.SetUiState(item.Value, ContractorUplinkUiKey.Key, state);
    }


    private void OnUiButtonPressed(Entity<ContractorUplinkComponent> ent, ref ContractorUiMessage msg) // extract the switch
    {
        if (!_net.IsServer)
            return;

        var user = msg.Actor;
        if (!Exists(user))
            return;

        if (!TryComp<ContractorComponent>(user, out var contractorComponent))
            return;

        var loc = msg.Location;

        switch (msg.Button)
        {
            case UiMessage.SelectTarget:
                UpdateContractorTarget(msg, contractorComponent, loc);
                break;
            case UiMessage.TryExtraction:
                StartExtraction(ent, user, contractorComponent);
                break;
            case UiMessage.Refresh:
                Log.Info("Refresh requested");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        UpdateUi(msg.Actor);
    }
     private void OnExtractionDoAfter(Entity<ContractorUplinkComponent> ent, ref ExtractionDoAfterEvent args)
    {
        if (args.Handled || !_net.IsServer)
            return;

        if (args.Cancelled)
            return;
        var flare = SpawnAtPosition(ent.Comp.Flare, GetTileInFrontOfEntity(args.User));
        _transform.SetLocalRotation(flare, Angle.Zero);
        ent.Comp.FlareUid = GetNetEntity(flare);
        ent.Comp.PortalSpawnTimer = _gameTiming.CurTime + ent.Comp.PortalSpawnTime;
        args.Handled = true;
    }



    private void StartExtraction(Entity<ContractorUplinkComponent> ent,
        EntityUid user,
        ContractorComponent contractorComponent)
    {
        ent.Comp.ExtractionCooldown = _gameTiming.CurTime + TimeSpan.FromSeconds(10);
        // light flare do after
        var lookup = _lookupSystem.GetEntitiesInRange<ContractorMarkerComponent>(user.ToCoordinates(), 8.0f);
        if (!lookup.Select(entity => entity.Owner)
                .Contains(GetEntity(contractorComponent.CurrentExtractionPoint)))
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager,
            user,
            3.0f,
            new ExtractionDoAfterEvent(),
            ent.Owner,
            used: ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfterEventArgs))
            return;
        return;
    }

    private void UpdateContractorTarget(ContractorUiMessage msg, ContractorComponent contractorComponent, NetEntity loc)
    {
        if (contractorComponent.Contracts.ContainsKey(msg.Target))
            contractorComponent.CurrentTarget = msg.Target;
        if (contractorComponent.Contracts.Values.Any(subList => subList.Contains(loc)))
            contractorComponent.CurrentExtractionPoint = msg.Location;
        if (!TryComp<ContractorMarkerComponent>(GetEntity(msg.Location), out var contractorMarkerComponent))
            return;
        contractorComponent.TcReward = contractorMarkerComponent.TcReward;
    }
    private void OnUplinkEquipped(Entity<ContractorUplinkComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!_net.IsServer || ent.Comp.User != NetEntity.Invalid)
            return;

        EnsureComp<ContractorComponent>(args.User);
        ent.Comp.User = GetNetEntity(args.User);
    }

}
