using Content.Shared._White.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Xenomorph;

public sealed partial class QueenRoarSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QueenRoarComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<QueenRoarComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<QueenRoarComponent, QueenRoarActionEvent>(OnQueenRoar);
        SubscribeLocalEvent<QueenRoarComponent, QueenRoarDoAfterEvent>(OnQueenRoarDoAfter);
    }

    private void OnMapInit(EntityUid uid, QueenRoarComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.RoarActionEntity, component.RoarAction);
    }

    private void OnShutdown(EntityUid uid, QueenRoarComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.RoarActionEntity);
    }

    private void OnQueenRoar(EntityUid uid, QueenRoarComponent component, QueenRoarActionEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupPredicted(
            Loc.GetString("queen-roar-start"),
            Loc.GetString("queen-roar-start-others"),
            uid,
            uid,
            PopupType.LargeCaution);

        var doAfter = new DoAfterArgs(EntityManager, args.Performer, component.RoarDelay, new QueenRoarDoAfterEvent(), uid)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
            MultiplyDelay = false,
        };

        _doAfter.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void OnQueenRoarDoAfter(EntityUid uid, QueenRoarComponent component, QueenRoarDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        _audio.PlayPredicted(component.SoundRoar, uid, args.User);

        var xform = Transform(uid);
        var nearMobs = _lookup.GetEntitiesInRange<NpcFactionMemberComponent>(xform.Coordinates, component.RoarRange, LookupFlags.Uncontained);

        foreach (var mob in nearMobs)
        {
            // Don't stun friendly entities
            if (_faction.IsEntityFriendly(uid, (mob.Owner, mob.Comp)))
                continue;

            // Stun
            _stun.TryStun(mob, TimeSpan.FromSeconds(component.RoarStunTime), true);

            // Show popup to the victim
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("queen-roar-victim"), mob.Owner, mob.Owner, PopupType.LargeCaution);
        }

        // Show popup to the queen
        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("queen-roar-complete"), args.User, args.User, PopupType.MediumCaution);

        args.Handled = true;
    }
}
