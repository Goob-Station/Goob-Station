using Content.Server.Actions;
using Content.Server.Hands.Systems;
using Content.Shared._Lavaland.Damage;
using Content.Shared.Actions;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared._Lavaland.Mobs.Components;
using Content.Server.Chat.Systems;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Content.Server._Lavaland.Mobs.Hierophant;

public sealed class HierophandClubItemSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly HierophantSystem _hierophant = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantClubItemComponent, MapInitEvent>(OnClubInit);
        SubscribeLocalEvent<HierophantClubItemComponent, GetItemActionsEvent>(OnGetActions);

        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubActivateCrossEvent>(OnCreateCross);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubPlaceMarkerEvent>(OnPlaceMarker);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubTeleportToMarkerEvent>(OnTeleport);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubToggleTileMovementEvent>(OnToggleTileMovement);
    }

    private void OnClubInit(Entity<HierophantClubItemComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.CreateCrossActionEntity, ent.Comp.CreateCrossActionId);
        _actions.AddAction(ent, ref ent.Comp.PlaceMarkerActionEntity, ent.Comp.PlaceMarkerActionId);
        _actions.AddAction(ent, ref ent.Comp.TeleportToMarkerActionEntity, ent.Comp.TeleportToMarkerActionId);
        _actions.AddAction(ent, ref ent.Comp.ToggleTileMovementActionEntity, ent.Comp.ToggleTileMovementActionId);
    }

    private void OnGetActions(Entity<HierophantClubItemComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.CreateCrossActionEntity, ent.Comp.CreateCrossActionId);
        args.AddAction(ref ent.Comp.PlaceMarkerActionEntity, ent.Comp.PlaceMarkerActionId);
        args.AddAction(ref ent.Comp.TeleportToMarkerActionEntity, ent.Comp.TeleportToMarkerActionId);
        args.AddAction(ref ent.Comp.ToggleTileMovementActionEntity, ent.Comp.ToggleTileMovementActionId);
    }

    private void OnCreateCross(Entity<HierophantClubItemComponent> ent, ref HierophantClubActivateCrossEvent args)
    {
        if (args.Handled || !args.Target.IsValid(EntityManager))
            return;

        var uid = ent.Owner;
        var user = args.Performer;

        if (!_hands.IsHolding(user, uid, out _))
        {
            _popup.PopupEntity(Loc.GetString("dash-ability-not-held", ("item", uid)), user, user);
            return;
        }

        var targetCoords = args.Target.SnapToGrid(EntityManager, _mapMan);

        SpawnHierophantCross(user, targetCoords, ent.Comp);

        args.Handled = true;
    }

    private void OnPlaceMarker(Entity<HierophantClubItemComponent> ent, ref HierophantClubPlaceMarkerEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;

        QueueDel(ent.Comp.TeleportMarker);

        var position = Transform(args.Performer)
            .Coordinates
            .AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapMan);
        var marker = Spawn(ent.Comp.TeleportMarkerPrototype, position);

        ent.Comp.TeleportMarker = marker;

        _popup.PopupEntity("Teleportation point set.", user);

        AddImmunity(user);
        _hierophant.SpawnDamageBox(user, 1, false);

        args.Handled = true;
    }

    private void OnTeleport(Entity<HierophantClubItemComponent> ent, ref HierophantClubTeleportToMarkerEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.TeleportMarker == null)
        {
            _popup.PopupClient("Marker is not placed!", args.Performer, PopupType.MediumCaution);
            return;
        }

        var user = args.Performer;

        AddImmunity(user);
        _xform.SetCoordinates(user, Transform(ent.Comp.TeleportMarker.Value).Coordinates); // CROSS MAP TP!!!
        _hierophant.Blink(user, ent.Comp.TeleportMarker);
        args.Handled = true;
    }

    private void OnToggleTileMovement(Entity<HierophantClubItemComponent> ent, ref HierophantClubToggleTileMovementEvent args)
    {
        if (args.Handled && !TerminatingOrDeleted(args.Target))
            return;

        if (HasComp<HierophantBeatComponent>(args.Target))
            RemComp<HierophantBeatComponent>(args.Target);
        else
            EnsureComp<HierophantBeatComponent>(args.Target);

        _chat.TrySendInGameICMessage(args.Performer, Loc.GetString("action-hierophant-tile-movement-cast"), InGameICChatType.Speak, false);
        args.Handled = true;
    }

    private void SpawnHierophantCross(EntityUid owner, EntityCoordinates coords, HierophantClubItemComponent club)
    {
        AddImmunity(owner);

        // shitcode because i dont want to rewrite and break the code again
        var dummy = Spawn(null, coords);
        _hierophant.SpawnCross(dummy, club.CrossRange, 0f);
        _audio.PlayPvs(club.DamageSound, coords, AudioParams.Default.WithMaxDistance(10f));
        QueueDel(dummy);
    }

    private void AddImmunity(EntityUid uid, float time = 3f)
    {
        EnsureComp<DamageSquareImmunityComponent>(uid).HasImmunityUntil = _timing.CurTime + TimeSpan.FromSeconds(time);
    }
}
