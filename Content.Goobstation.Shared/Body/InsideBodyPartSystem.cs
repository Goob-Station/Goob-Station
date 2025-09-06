using Content.Goobstation.Common.Body;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Body;

public sealed class InsideBodyPartSystem : CommonInsideBodyPartSystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;

    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<BodyPartComponent> _partQuery;

    public override void Initialize()
    {
        base.Initialize();

        _bodyQuery = GetEntityQuery<BodyComponent>();
        _partQuery = GetEntityQuery<BodyPartComponent>();

        SubscribeLocalEvent<InsideBodyPartComponent, BodyPartBurstEvent>(OnAction);
        SubscribeLocalEvent<InsideBodyPartComponent, BurstDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<InsideBodyPartComponent, ComponentShutdown>(OnShutdown);
    }

    public override void InsertedIntoPart(EntityUid item, EntityUid part)
    {
        var comp = EnsureComp<InsideBodyPartComponent>(item);
        comp.Part = part;
        Dirty(item, comp);
        if (!_bodyQuery.HasComp(item))
            return;

        _actions.AddAction(item, ref comp.ActionEntity, comp.BurstAction);
        if (comp.ActionEntity is {} action)
            _actions.SetEntityIcon(action, part);
    }

    public override void RemovedFromPart(EntityUid item)
    {
        RemComp<InsideBodyPartComponent>(item);
    }

    private void OnAction(Entity<InsideBodyPartComponent> ent, ref BodyPartBurstEvent args)
    {
        var part = ent.Comp.Part;
        var body = _partQuery.Comp(part).Body;
        // it's easier to burst out of a corpse
        var delay = ent.Comp.Delay;
        if (body != null)
        {
            if (_mob.IsAlive(body.Value))
                delay = ent.Comp.AliveDelay;
            _jittering.DoJitter(body.Value, delay, refresh: true);
        }

        var ev = new BurstDoAfterEvent();
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent.Owner, delay, ev, eventTarget: ent, target: part));

        var victim = Identity.Name(body ?? part, EntityManager);
        _popup.PopupPredicted(Loc.GetString("body-part-burst-starting", ("victim", victim), ("part", part)), ent, ent, PopupType.LargeCaution);
    }

    private void OnDoAfter(Entity<InsideBodyPartComponent> ent, ref BurstDoAfterEvent args)
    {
        var part = ent.Comp.Part;
        if (args.Cancelled || !_partQuery.TryComp(part, out var partComp)) return;
            return;

        _slots.TryEject(part, partComp.ItemInsertionSlot, ent, out _);
        _damage.TryChangeDamage(part, ent.Comp.BurstDamage, ignoreResistances: true);

        var victim = Identity.Name(partComp.Body ?? part, EntityManager);
        _popup.PopupPredicted(Loc.GetString("body-part-burst-finished", ("victim", victim), ("burst", ent.Owner)), ent, ent, PopupType.LargeCaution);

        // this should never happen as container events should indirectly remove it, but just incase
        RemovedFromPart(ent);
    }

    private void OnShutdown(Entity<InsideBodyPartComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
    }
}
