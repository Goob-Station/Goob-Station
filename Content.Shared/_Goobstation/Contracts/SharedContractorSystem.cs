using System.Linq;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Pinpointer;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Contracts;

/// <summary>
/// This handles contracts for syndicate contractors.
/// </summary>
public sealed class SharedContractorSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // Init
        SubscribeLocalEvent<ContractorComponent, MapInitEvent>(OnContractorMapInit);
        SubscribeLocalEvent<ContractorMarkerComponent, MapInitEvent>(OnMarkerMapInit);

        // Events
        SubscribeLocalEvent<ContractorUplinkComponent, ContractorUiMessage>(OnUiButtonPressed);
        SubscribeLocalEvent<ContractorUplinkComponent, GotEquippedHandEvent>(OnUplinkEquipped);
        SubscribeLocalEvent<ContractorUplinkComponent, ExtractionDoAfterEvent>(OnExtractionDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ContractorUplinkComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_gameTiming.CurTime < comp.PortalSpawnTimer && comp.PortalSpawnTimer == TimeSpan.Zero)
                continue;

            // spawn portal

        }
    }

    private void OnExtractionDoAfter(Entity<ContractorUplinkComponent> ent, ref ExtractionDoAfterEvent args)
    {
        if (args.Handled || !_net.IsServer)
            return;

        if (args.Cancelled)
            return;

        var flare = SpawnAtPosition(ent.Comp.Flare, args.User.ToCoordinates());
        _transform.SetLocalRotation(flare, Angle.Zero);
        ent.Comp.FlareUid = flare;
        ent.Comp.PortalSpawnTimer = _gameTiming.CurTime + ent.Comp.PortalSpawnTime;
        args.Handled = true;

    }


    private void OnUplinkEquipped(Entity<ContractorUplinkComponent> ent, ref GotEquippedHandEvent args)
    {
        if(!_net.IsServer || ent.Comp.Used)
            return;

        EnsureComp<ContractorComponent>(args.User);
        ent.Comp.Used = true;
    }


    private void OnMarkerMapInit(Entity<ContractorMarkerComponent> ent, ref MapInitEvent args)
    {
        var entitiesIntersecting = _lookupSystem.GetEntitiesIntersecting(ent.Owner.ToCoordinates());
        foreach (var entity in entitiesIntersecting)
        {
            if (!TryComp<NavMapBeaconComponent>(entity, out var navMapBeaconComponent))
                continue;

            if (navMapBeaconComponent.DefaultText == null)
                continue;

            ent.Comp.Name = navMapBeaconComponent.DefaultText;
            ent.Comp.TcReward = 4; // de-hardcode lol
            Dirty(ent, ent.Comp);
            break;
        }
    }

    private void OnContractorMapInit(Entity<ContractorComponent> ent, ref MapInitEvent args)
    {
        SetupContracts(ent);
        UpdateUi(ent);
    }

    private void UpdateUi(EntityUid uid)
    {
        if (!_handsSystem.TryGetActiveItem(uid, out var item) || !_userInterfaceSystem.HasUi(item.Value, ContractorUplinkUiKey.Key) || !TryComp<ContractorComponent>(uid, out var contractorComponent))
            return;

        var state = new ContractorUplinkBoundUserInterfaceState(contractorComponent.Tc,
            contractorComponent.Contracts,
            contractorComponent.Rep,
            contractorComponent.CurrentTarget,
            contractorComponent.CurrentExtractionPoint);

        _userInterfaceSystem.SetUiState(item.Value, ContractorUplinkUiKey.Key, state);
    }

    private void OnUiButtonPressed(Entity<ContractorUplinkComponent> ent, ref ContractorUiMessage msg) // extract the switch
    {
        if (!_net.IsServer)
            return;

        var user = msg.Actor;
        if (!Exists(user))
            return;

        if(!TryComp<ContractorComponent>(user, out var contractorComponent))
            return;

        var loc = msg.Location;

        switch (msg.Button)
        {
            case UiMessage.SelectTarget:
                if(contractorComponent.Contracts.ContainsKey(msg.Target))
                    contractorComponent.CurrentTarget = msg.Target;
                if(contractorComponent.Contracts.Values.Any(subList => subList.Contains(loc))) // this is evil
                    contractorComponent.CurrentExtractionPoint = msg.Location;
                if (!TryComp<ContractorMarkerComponent>(GetEntity(msg.Location), out var contractorMarkerComponent))
                    return;
                contractorComponent.TcReward = contractorMarkerComponent.TcReward;
                break;
            case UiMessage.TryExtraction:
                // light flare do after
                var lookup = _lookupSystem.GetEntitiesInRange<ContractorMarkerComponent>(user.ToCoordinates(), 8.0f);
                if (!lookup.Select(entity => entity.Owner)
                    .Contains(GetEntity(contractorComponent.CurrentExtractionPoint)))
                    break;

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

                break;
            case UiMessage.Refresh:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        UpdateUi(msg.Actor);
    }

    private void SetupContracts(Entity<ContractorComponent> ent)
    {
        if (!_net.IsServer) // I really feel like this is the really lazy way of doing this instead of moving it to server but... alas we are here.
            return;

        // get ready for the craziest linq maxing
        var possibleContracts = _mindSystem.GetAliveHumans();

        foreach (var humanoid in possibleContracts)
        foreach (var mindRole in humanoid.Comp.MindRoles) // cleanse the list of yourself
            if (!TryComp<MindRoleComponent>(mindRole, out var mindRoleComp) || mindRoleComp.Antag || //allow antags
                humanoid.Comp.OwnedEntity == ent.Owner || humanoid.Comp.OwnedEntity == null)
                possibleContracts.Remove(humanoid);

        if (possibleContracts.Count == 0)
        {
            Log.Debug("Not enough alive humanoids to generate a contract"); // add a refresh timer or sum IDK need a brain
            return;
        }

        var targets = possibleContracts.OrderBy(_ => _random.Next())
            .Take(5)
            .Select(entity => GetNetEntity(entity.Comp.OwnedEntity))
            .ToList();

        var query = EntityQueryEnumerator<ContractorMarkerComponent>();
        var markerList = new List<NetEntity>();

        while (query.MoveNext(out var uid, out _))
        {
            markerList.Add(GetNetEntity(uid));
        }

        foreach (var target in targets)
        {
            if (target == null)
                return;

            ent.Comp.Contracts.Add(target.Value, markerList.OrderBy(_ => _random.Next()).Take(3).ToList());
            Dirty(ent, ent.Comp); // dirty since this is the server
        }
    }
}
