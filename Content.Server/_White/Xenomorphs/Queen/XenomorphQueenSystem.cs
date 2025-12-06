using Content.Server._White.Xenomorphs.Evolution;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._White.Actions;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Queen;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Server.Stunnable; //goob
using Content.Shared.Mind.Components;
using Content.Shared.NPC.Components; //Goob
using Content.Shared.NPC.Systems; //Goob
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems; //Goob

namespace Content.Server._White.Xenomorphs.Queen;

public sealed class XenomorphQueenSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PlasmaSystem _plasma = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly XenomorphEvolutionSystem _xenomorphEvolution = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly StunSystem _stun = default!; // Goobstation
    [Dependency] private readonly EntityLookupSystem _lookup = default!; // Goobstation
    [Dependency] private readonly NpcFactionSystem _faction = default!; //Goob
    [Dependency] private readonly SharedAudioSystem _audio = default!; //goob

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphQueenComponent, PromotionActionEvent>(OnPromotionAction);
        SubscribeLocalEvent<XenomorphQueenComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<XenomorphQueenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphQueenComponent, QueenRoarActionEvent>(OnQueenRoar); // Goobstation, copied from dragon roar
    }

    private void OnMapInit(EntityUid uid, XenomorphQueenComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.PromotionAction, component.PromotionActionId);
        _actions.AddAction(uid, ref component.RoarActionEntity, component.RoarAction); // Goobstation, copied from dragon code
    }

    private void OnShutdown(EntityUid uid, XenomorphQueenComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.PromotionAction);

    private void OnPromotionAction(EntityUid uid, XenomorphQueenComponent component, PromotionActionEvent args)
    {
        // Goobstation start
        if (args.Target == EntityUid.Invalid || args.Target == args.Performer)
            return;

        // Additional validation in case the target is no longer valid
        if (!HasComp<XenomorphComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-invalid-target"), args.Performer);
            return;
        }

        if (!TryComp<XenomorphComponent>(args.Target, out var xenomorph))
            return;

        // Check if target is already a Praetorian or not in the whitelist
        if (xenomorph.Caste == "Praetorian" || !component.CasteWhitelist.Contains(xenomorph.Caste))
        {
            if (xenomorph.Caste == "Praetorian")
                _popup.PopupEntity(Loc.GetString("xenomorphs-queen-already-praetorian"), args.Performer);
            else
                _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-didnt-pass-whitelist"), args.Performer);
            return;
        }

        // Try direct evolution with optional mind transfer
        var target = args.Target;
        var coordinates = Transform(target).Coordinates;
        var newXeno = Spawn(component.PromoteTo, coordinates);

        // Transfer mind if it exists
        if (_mind.TryGetMind(target, out var mindId, out var mind))
            _mind.TransferTo(mindId, newXeno, mind: mind);

        // Copy over any important components
        if (TryComp<XenomorphComponent>(newXeno, out var newXenoComp) &&
            TryComp<XenomorphComponent>(target, out var oldXenoComp))
        {
            newXenoComp.Caste = oldXenoComp.Caste;
        }

        // Update the caste to Praetorian for the new entity
        if (TryComp<XenomorphComponent>(newXeno, out var xenomorphComp))
        {
            xenomorphComp.Caste = "Praetorian";
            Dirty(newXeno, xenomorphComp);
        }

        // Get the target's name before deleting the entity
        var targetName = Name(target);

        // Clean up the old entity
        Del(target);

        // Deduct plasma cost if applicable
        _plasma.ChangePlasmaAmount(uid, -500f); // Deduct 500 plasma for the promotion
        _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-success", ("target", targetName)), uid, uid);
        args.Handled = true;
    }
//I stole this from dragon
    private void OnQueenRoar(EntityUid uid, XenomorphQueenComponent component, QueenRoarActionEvent args)
    {
        if (args.Handled)
            return;

        Roar(uid, component);

        var xform = Transform(uid);
        var nearMobs = _lookup.GetEntitiesInRange<NpcFactionMemberComponent>(xform.Coordinates, component.RoarRange, LookupFlags.Uncontained);
        foreach (var mob in nearMobs)
        {
            if (_faction.IsEntityFriendly(uid, (mob.Owner, mob.Comp)))
                continue;

            _stun.TryStun(mob, TimeSpan.FromSeconds(component.RoarStunTime), false);
        }

        args.Handled = true;
    }

    private void Roar(EntityUid uid, XenomorphQueenComponent comp)
    {
        if (comp.SoundRoar != null)
            _audio.PlayPvs(comp.SoundRoar, uid);
    }// Goobstation end
}
