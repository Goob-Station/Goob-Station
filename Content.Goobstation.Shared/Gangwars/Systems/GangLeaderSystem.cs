using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Gangwars.Systems;

public sealed class GangLeaderSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly GangwarRuleSystem _gangwarRule = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    private static readonly EntProtoId DropPodSpawner = "DropPodspawner";
    private static readonly EntProtoId GangMemberMindRole = "MindRoleGangMember";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangLeaderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<GangLeaderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GangLeaderComponent, GangLeaderSummonLockerEvent>(OnSummonLocker);
        SubscribeLocalEvent<GangLeaderComponent, GangLeaderMemberOfferEvent>(OnMemberOffer);
        SubscribeLocalEvent<GangLockerComponent, EntRemovedFromContainerMessage>(OnLockerRemovedFromContainer);
        SubscribeNetworkEvent<GangColorChosenEvent>(OnColorChosen);
        SubscribeNetworkEvent<GangInviteResponseEvent>(OnInviteResponse);
    }

    private void OnMapInit(Entity<GangLeaderComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.SummonLockerAction);
        Dirty(ent);
    }

    private void OnShutdown(Entity<GangLeaderComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
        _actions.RemoveAction(ent.Owner, ent.Comp.MemberOfferActionEnt);
    }

    private void OnSummonLocker(Entity<GangLeaderComponent> ent, ref GangLeaderSummonLockerEvent args)
    {
        var coords = Transform(ent.Owner).Coordinates.SnapToGrid();

        if (IsNearGangCrate(coords, ent))
        {
            _popup.PopupClient(Loc.GetString("gang-locker-too-close-to-crate"), ent.Owner, ent.Owner);
            return;
        }

        if (_netManager.IsServer
            && ent.Comp.GangLocker == null
            && TryComp<ActorComponent>(ent.Owner, out var actor))
        {
            _gangwarRule.TryGetRule(out _, out var gangwarRule);
            RaiseNetworkEvent(new GangLeaderNeedsColorPickEvent(gangwarRule.GangNames), actor.PlayerSession);
            return;
        }

        var defaultRadius = new GangTerritoryComponent().TerritoryRadius;

        if (!TryComp<GangMemberComponent>(ent.Owner, out var gangMember)
            || _gangwarRule.IsTerritoryTooClose(_xform.ToMapCoordinates(coords), defaultRadius, gangMember?.Gang))
        {
            _popup.PopupClient(Loc.GetString("gang-territory-too-close"), ent.Owner, ent.Owner);
            return;
        }

        args.Handled = true;

        // If the locker already exists move it instead of making a new one
        if (ent.Comp.GangLocker != null && !TerminatingOrDeleted(ent.Comp.GangLocker))
            PlaceLockerInDropPod(ent, ent.Comp.GangLocker.Value, coords);

        // This one makes the locker if it doesn't exist somehow???
        else if (gangMember?.Gang != null)
            SpawnGangLocker(ent, gangMember.Gang.Value, coords);
    }

    /// <summary>
    /// Happens when someone picks a color in the ui
    /// </summary>
    private void OnColorChosen(GangColorChosenEvent ev, EntitySessionEventArgs args)
    {
        var playerEntity = args.SenderSession.AttachedEntity;
        if (playerEntity == null
            || !TryComp<GangLeaderComponent>(playerEntity, out var leaderComp)
            || !TryComp<GangMemberComponent>(playerEntity, out var gangMemberComp))
            return;

        if (leaderComp.PendingRemakeColor is { } remakeColor)
        {
            var remakeName = ev.GangName.Trim();
            if (!IsValidGangAppearance(ev.ChosenColor, remakeName)
                || _gangwarRule.IsColorOrNameTakenByOther(remakeColor, ev.ChosenColor, remakeName))
                return;

            leaderComp.PendingRemakeColor = null;
            Dirty(playerEntity.Value, leaderComp);

            _gangwarRule.MigrateGang(remakeColor, ev.ChosenColor, remakeName);

            _adminLog.Add(LogType.Action, LogImpact.High,
                $"{ToPrettyString(playerEntity.Value):player} remade their gang into \"{remakeName}\" with color {ev.ChosenColor.ToHex()}");
            return;
        }

        if (leaderComp.GangLocker != null)
            return;

        var coords = Transform(playerEntity.Value).Coordinates.SnapToGrid();
        var defaultRadius = new GangTerritoryComponent().TerritoryRadius;
        if (_gangwarRule.IsTerritoryTooClose(_xform.ToMapCoordinates(coords), defaultRadius))
        {
            _popup.PopupEntity(Loc.GetString("gang-territory-too-close"), playerEntity.Value, playerEntity.Value);
            return;
        }

        if (IsNearGangCrate(coords, (playerEntity.Value, leaderComp)))
        {
            _popup.PopupEntity(Loc.GetString("gang-locker-too-close-to-crate"), playerEntity.Value, playerEntity.Value);
            return;
        }

        // And so I said "No modified clients shall ignore my restrictions"
        var gangName = ev.GangName.Trim();
        if (!IsValidGangAppearance(ev.ChosenColor, gangName)
            || _gangwarRule.IsColorOrNameTakenByOther(null, ev.ChosenColor, gangName))
            return;

        if (_gangwarRule.TryGetRule(out var gangwarRuleEntity, out var gangwarRule))
        {
            gangwarRule.GangNames[ev.ChosenColor] = gangName;
            Dirty(gangwarRuleEntity, gangwarRule);
        }

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(playerEntity.Value):player} founded the gang \"{gangName}\" with color {ev.ChosenColor.ToHex()}");

        gangMemberComp.Gang = ev.ChosenColor;
        gangMemberComp.GangName = gangName;
        Dirty(playerEntity.Value, gangMemberComp);

        _actions.AddAction(playerEntity.Value, ref leaderComp.MemberOfferActionEnt, leaderComp.MemberOfferAction);
        Dirty(playerEntity.Value, leaderComp);

        SpawnGangLocker((playerEntity.Value, leaderComp), ev.ChosenColor, coords);
    }

    /// <summary>
    /// Spawns a new gang locker for the leader, applies all gang components, and drops it via pod.
    /// </summary>
    private void SpawnGangLocker(Entity<GangLeaderComponent> leader, Color color, EntityCoordinates coords)
    {
        var gangLocker = Spawn(leader.Comp.GangLockerPrototype);

        leader.Comp.GangLocker = gangLocker;
        Dirty(leader);

        if (TryComp<GangLockerComponent>(gangLocker, out var lockerComp))
        {
            lockerComp.OwnerLeader = leader.Owner;
            lockerComp.GangColor = color;
            Dirty(gangLocker, lockerComp);
        }

        var gangColor = EnsureComp<GangColorComponent>(gangLocker);
        gangColor.GangColor = color;
        Dirty(gangLocker, gangColor);

        var territoryComp = EnsureComp<GangTerritoryComponent>(gangLocker);
        territoryComp.GangColor = color;
        territoryComp.BuffedHealing = true;
        Dirty(gangLocker, territoryComp);

        _actions.StartUseDelay(leader.Comp.ActionEnt);
        _popup.PopupClient(Loc.GetString("gang-leader-locker-summoned"), leader.Owner, leader.Owner);

        PlaceLockerInDropPod(leader, gangLocker, coords);
    }

    /// <summary>
    /// Places the gang locker inside a DropPodspawner and triggers it.
    /// </summary>
    private void PlaceLockerInDropPod(Entity<GangLeaderComponent> _, EntityUid locker, EntityCoordinates coords)
    {
        if (!_netManager.IsServer)
            return;

        _xform.Unanchor(locker);

        var pod = Spawn(DropPodSpawner, coords);

        if (_containers.TryGetContainer(pod, "clowncar_container", out var container)) // I don't know why drop pods use clowncar_container. It's just the magic of goobcode
            _containers.Insert(locker, container);

        _trigger.Trigger(pod);
    }
    private void OnLockerRemovedFromContainer(Entity<GangLockerComponent> ent, ref EntRemovedFromContainerMessage args) =>
        _xform.AnchorEntity(ent);

    #region Gang invites
    private void OnMemberOffer(Entity<GangLeaderComponent> ent, ref GangLeaderMemberOfferEvent args)
    {
        var target = args.Target;

        if (args.Handled == true
           || !TryComp<GangMemberComponent>(ent.Owner, out var gangMember)
           || gangMember.Gang == null
           || !TryComp<ActorComponent>(target, out var targetActor))
            return;

        args.Handled = true;

        if (ent.Comp.PendingInviteTarget != null)
        {
            _popup.PopupClient(Loc.GetString("gang-invite-already-outgoing"), ent.Owner, ent.Owner);
            return;
        }

        if (HasComp<GangMemberComponent>(target))
        {
            _popup.PopupClient(Loc.GetString("gang-invite-target-already-member"), ent.Owner, ent.Owner);
            return;
        }

        ent.Comp.PendingInviteTarget = target;
        Dirty(ent);

        if (_netManager.IsServer)
        {
            RaiseNetworkEvent(new GangInviteOfferEvent(Name(ent.Owner), gangMember.Gang.Value, GetNetEntity(ent.Owner), gangMember.GangName ?? string.Empty), targetActor.PlayerSession);
        }

        _popup.PopupClient(Loc.GetString("gang-invite-sent", ("name", Name(target))), ent.Owner, ent.Owner);
    }

    private void OnInviteResponse(GangInviteResponseEvent ev, EntitySessionEventArgs args)
    {
        var leaderUid = GetEntity(ev.LeaderEntity);
        var responder = args.SenderSession.AttachedEntity;

        if (responder == null
            || !TryComp<GangLeaderComponent>(leaderUid, out var leaderComp)
            || leaderComp.PendingInviteTarget != responder)
            return;

        if (!ev.Accepted)
        {
            _popup.PopupEntity(Loc.GetString("gang-leader-member-denied"), leaderUid, leaderUid);
            leaderComp.PendingInviteTarget = null;
            Dirty(leaderUid, leaderComp);
            return;
        }

        if (TryComp<GangMemberComponent>(leaderUid, out var leaderMember) && leaderMember.Gang != null)
            AcceptGangInvite((leaderUid, leaderComp), responder.Value, leaderMember.Gang.Value, leaderMember.GangName ?? string.Empty);

        leaderComp.PendingInviteTarget = null;
        Dirty(leaderUid, leaderComp);
    }

    private void AcceptGangInvite(Entity<GangLeaderComponent> leader, EntityUid recruit, Color gangColor, string gangName)
    {
        var gangMember = EnsureComp<GangMemberComponent>(recruit);
        gangMember.Gang = gangColor;
        gangMember.GangName = gangName;
        Dirty(recruit, gangMember);

        leader.Comp.InvitesUsed++;
        if (leader.Comp.InvitesUsed >= leader.Comp.MaxInvites)
            _actions.RemoveAction(leader.Owner, leader.Comp.MemberOfferActionEnt);
        Dirty(leader);

        if (_mind.TryGetMind(recruit, out var mindId, out _))
            _role.MindAddRole(mindId, GangMemberMindRole, null, true);

        RaiseLocalEvent(recruit, new GangMemberRecruitedEvent());

        _popup.PopupEntity(Loc.GetString("gang-leader-member-accepted"), leader.Owner, leader.Owner);
    }

    #endregion

    private bool IsNearGangCrate(EntityCoordinates coords, Entity<GangLeaderComponent> ent) =>
        _lookup.GetEntitiesInRange<GangCrateComponent>(coords, ent.Comp.CrateExclusionZone).Count > 0;

    private static bool IsValidGangAppearance(Color color, string name)
    {
        return color.A >= 0.9f
            && Color.ToHsv(color).Z >= 0.70f
            && name.Length >= 4
            && name.Length <= 17;
    }

}
