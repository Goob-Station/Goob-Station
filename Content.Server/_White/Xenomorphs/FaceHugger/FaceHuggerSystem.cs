using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._White.Xenomorphs.FaceHugger;

public sealed class FaceHuggerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaceHuggerComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<FaceHuggerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<FaceHuggerComponent, GotEquippedHandEvent>(OnPickedUp);

        SubscribeLocalEvent<FaceHuggerComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<FaceHuggerComponent, BeingUnequippedAttemptEvent>(OnBeingUnequippedAttempt);
    }

    private void OnCollideEvent(EntityUid uid, FaceHuggerComponent component, StartCollideEvent args)
        => TryEquipFaceHugger(uid, args.OtherEntity, component);

    private void OnMeleeHit(EntityUid uid, FaceHuggerComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.FirstOrNull() is not {} target)
            return;

        TryEquipFaceHugger(uid, target, component);
    }

    private void OnPickedUp(EntityUid uid, FaceHuggerComponent component, GotEquippedHandEvent args)
        => TryEquipFaceHugger(uid, args.User, component);

    private void OnGotEquipped(EntityUid uid, FaceHuggerComponent component, GotEquippedEvent args)
    {
        if (args.Slot != component.Slot
            || _mobState.IsDead(uid)
            || _entityWhitelist.IsBlacklistPass(component.Blacklist, args.Equipee))
            return;

        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-equip", ("equipment", uid)), uid, args.Equipee);
        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-equip-other", ("equipment", uid), ("target", Identity.Entity(args.Equipee, EntityManager))), uid, Filter.PvsExcept(args.Equipee), true);

        _stun.TryKnockdown(args.Equipee, component.KnockdownTime, true);

        if (!component.InfectionPrototype.HasValue)
            return;

        component.InfectIn = _timing.CurTime + _random.Next(component.MinInfectTime, component.MaxInfectTime);
    }

    private void OnBeingUnequippedAttempt(EntityUid uid, FaceHuggerComponent component, BeingUnequippedAttemptEvent args)
    {
        if (_mobState.IsDead(uid) || !component.InfectionPrototype.HasValue || args.Unequipee != args.UnEquipTarget)
            return;

        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-unequip", ("equipment", Identity.Entity(uid, EntityManager))), uid, args.Unequipee);
        args.Cancel();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<FaceHuggerComponent>();
        while (query.MoveNext(out var uid, out var faceHugger))
        {
            if (!faceHugger.Active && time > faceHugger.RestIn)
                faceHugger.Active = true;

            if (faceHugger.InfectIn != TimeSpan.Zero && time > faceHugger.InfectIn)
            {
                faceHugger.InfectIn = TimeSpan.Zero;
                Infect(uid, faceHugger);
            }
        }
    }

    private void Infect(EntityUid uid, FaceHuggerComponent component)
    {
        if (!component.InfectionPrototype.HasValue
            || !TryComp<ClothingComponent>(uid, out var clothing)
            || clothing.InSlot != component.Slot
            || !_container.TryGetContainingContainer((uid, null, null), out var target))
            return;

        var bodyPart = _body.GetBodyChildrenOfType(target.Owner, component.InfectionBodyPart.Type, symmetry: component.InfectionBodyPart.Symmetry).FirstOrNull();
        if (!bodyPart.HasValue)
            return;

        var organ = Spawn(component.InfectionPrototype);
        _body.TryCreateOrganSlot(bodyPart.Value.Id, component.InfectionSlotId, out _, bodyPart.Value.Component);

        if (!_body.InsertOrgan(bodyPart.Value.Id, organ, component.InfectionSlotId, bodyPart.Value.Component))
        {
            QueueDel(organ);
            return;
        }

        _damageable.TryChangeDamage(uid, component.DamageOnInfect, true);
    }

    public bool TryEquipFaceHugger(EntityUid uid, EntityUid target, FaceHuggerComponent component)
    {
        if (!component.Active || _mobState.IsDead(uid) || _entityWhitelist.IsBlacklistPass(component.Blacklist, target))
            return false;

        component.RestIn = _timing.CurTime + _random.Next(component.MinRestTime, component.MaxRestTime);
        component.Active = false;

        EntityUid? blocker = null;

        if (_inventory.TryGetSlotEntity(target, "head", out var headUid)
            && TryComp<IngestionBlockerComponent>(headUid, out var headBlocker)
            && headBlocker.Enabled)
            blocker = headUid;

        if (!blocker.HasValue && _inventory.TryGetSlotEntity(target, "mask", out var maskUid))
        {
            if (TryComp<IngestionBlockerComponent>(maskUid, out var maskBlocker) && maskBlocker.Enabled)
                blocker = maskUid;
            else
                _inventory.TryUnequip(target, component.Slot, true);
        }

        if (!blocker.HasValue)
            return _inventory.TryEquip(target, uid, component.Slot, true, true);

        _audio.PlayPvs(component.SoundOnImpact, uid);

        _damageable.TryChangeDamage(uid, component.DamageOnImpact);

        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-try-equip", ("equipment", uid), ("equipmentBlocker", blocker.Value)), uid);
        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-try-equip-other", ("equipment", uid), ("equipmentBlocker", blocker.Value), ("target", Identity.Entity(target, EntityManager))), uid, Filter.PvsExcept(target), true);

        return false;
    }
}
