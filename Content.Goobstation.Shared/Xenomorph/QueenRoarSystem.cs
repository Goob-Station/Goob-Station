using Content.Shared._White.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Xenomorph;

public sealed partial class QueenRoarSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private NpcFactionSystem _faction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;

    private readonly HashSet<Entity<NpcFactionMemberComponent>> _nearbyMobs = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QueenRoarComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<QueenRoarComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<QueenRoarComponent, QueenRoarActionEvent>(OnQueenRoar);
        SubscribeLocalEvent<QueenRoarComponent, QueenRoarDoAfterEvent>(OnQueenRoarDoAfter);
    }

    private void OnInit(Entity<QueenRoarComponent> ent, ref ComponentInit args)
    {
        ent.Comp.RoarActionEntity = GetNetEntity(_actions.AddAction(ent.Owner, ent.Comp.RoarAction));
    }

    private void OnShutdown(Entity<QueenRoarComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, GetEntity(ent.Comp.RoarActionEntity));

    private void OnQueenRoar(Entity<QueenRoarComponent> ent, ref QueenRoarActionEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupPredicted(
            Loc.GetString("queen-roar-start"),
            Loc.GetString("queen-roar-start-others"),
            ent.Owner,
            ent.Owner,
            PopupType.LargeCaution);

        _audio.PlayPredicted(ent.Comp.SoundRoarStart, ent.Owner, args.Performer);

        var doAfter = new DoAfterArgs(EntityManager, args.Performer, ent.Comp.RoarDelay, new QueenRoarDoAfterEvent(), ent.Owner)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
            MultiplyDelay = false,
        };

        _doAfter.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void OnQueenRoarDoAfter(Entity<QueenRoarComponent> ent, ref QueenRoarDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        _audio.PlayPredicted(ent.Comp.SoundRoar, ent.Owner, args.User);

        var xform = Transform(ent.Owner);
        _nearbyMobs.Clear();
        _lookup.GetEntitiesInRange(xform.Coordinates, ent.Comp.RoarRange, _nearbyMobs, LookupFlags.Uncontained);

        foreach (var mob in _nearbyMobs)
        {
            if (_faction.IsEntityFriendly(ent.Owner, (mob.Owner, mob.Comp)))
                continue;

            if (!_interaction.InRangeUnobstructed(ent.Owner, mob.Owner, ent.Comp.RoarRange))
                continue;

            _stun.KnockdownOrStun(mob, ent.Comp.RoarStunTime);
        }

        _popup.PopupPredicted(Loc.GetString("queen-roar-complete"), ent.Owner, args.User, PopupType.MediumCaution);

        args.Handled = true;
    }
}
