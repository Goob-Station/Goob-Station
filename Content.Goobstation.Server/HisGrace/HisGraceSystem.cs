using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Hands;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.HisGrace;

// todo!!
// - clamp hunger between 0 and 200
// - make healing work
// - popups and flavor
// - attack anim while on ground. probs how melee does it.
// - ascension - thermals and speed up
public sealed partial class HisGraceSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _state = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HisGraceComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<HisGraceComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<HisGraceComponent, GotUnequippedHandEvent>(OnUnequipped);
        SubscribeLocalEvent<HisGraceComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<HisGraceComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<HisGraceComponent, HisGraceHungerChangedEvent>(OnHungerChanged);
        SubscribeLocalEvent<HisGraceComponent, HisGraceEntityConsumedEvent>(OnEntityConsumed);
    }

    private void OnInit(EntityUid uid, HisGraceComponent component, MapInitEvent args)
    {
        component.Stomach = _containerSystem.EnsureContainer<Container>(uid, "stomach");

        if (!TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        component.BaseDamage = melee.Damage;
    }

    private void OnEquipped(EntityUid uid, HisGraceComponent component, ref GotEquippedHandEvent args)
    {
        component.IsHeld = true;
    }

    private void OnUnequipped(EntityUid uid, HisGraceComponent component, ref GotUnequippedHandEvent args)
    {
        component.IsHeld = false;
    }

    private void OnMeleeHit(EntityUid uid, HisGraceComponent comp, ref MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
        {
            if (_state.IsIncapacitated(hitEntity))
            {
                _containerSystem.Insert(hitEntity, comp.Stomach);

                var ev = new HisGraceEntityConsumedEvent();
                RaiseLocalEvent(uid, ref ev);
            }
        }

    }

    private void OnUse(EntityUid uid, HisGraceComponent comp, ref UseInHandEvent args)
    {
        if (comp.User is not null)
            return;

        comp.User = args.User;
        EnsureComp<HisGraceUserComponent>(args.User);

        ChangeState(comp, HisGraceState.Peckish);
    }

    private void OnEntityConsumed(EntityUid uid, HisGraceComponent comp, ref HisGraceEntityConsumedEvent args)
    {
        comp.EntitiesAbsorbed++;
        comp.Hunger -= comp.HungerOnDevour;

        if (comp.EntitiesAbsorbed >= 25)
            ChangeState(comp, HisGraceState.Ascended);

        if (comp.EntitiesAbsorbed <= 0 || !TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        comp.CurrentDamageIncrease = comp.EntitiesAbsorbed * 5;
        melee.Damage.DamageDict["Blunt"] = comp.BaseDamage.DamageDict["Blunt"] + comp.CurrentDamageIncrease; // unhard code damage type if maintainer bitches
    }

    private void OnHungerChanged(EntityUid uid, HisGraceComponent comp, ref HisGraceHungerChangedEvent args)
    {
        switch (args.NewState)
        {
            case HisGraceState.Dormant:
            case HisGraceState.Peckish:
            case HisGraceState.Hungry:
            {
                RemComp<UnremoveableComponent>(uid);
                RemComp<JitteringComponent>(uid);
                break;
            }

            case HisGraceState.Ravenous:
            case HisGraceState.Starving:
            {
                EnsureComp<UnremoveableComponent>(uid);
                EnsureComp<JitteringComponent>(uid);
                break;
            }

            case HisGraceState.Death:
            {
                if (comp.User is not { } user)
                    return;

                RemComp<UnremoveableComponent>(uid);

                comp.User = null;
                _damageable.TryChangeDamage(user, comp.DamageOnFail, targetPart: TargetBodyPart.Head,  origin: uid, ignoreResistances: true);
                _hands.TryDrop(user);

                EnsureComp<JitteringComponent>(uid);
                break;
            }

            case HisGraceState.Ascended:
            {
                EnsureComp<UnremoveableComponent>(uid);
                break;
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HisGraceComponent>();
        while (query.MoveNext(out var uid, out var hisGrace))
        {
            if (hisGrace.CurrentState == HisGraceState.Dormant)
                continue;

            if (!hisGrace.IsHeld)
            {
                var nearbyEnts = _lookup.GetEntitiesInRange(uid, 1f);
                foreach (var entity in nearbyEnts.Where(entity => HasComp<MobStateComponent>(entity) && entity != hisGrace.User && hisGrace.CanAttack))
                {
                    _damageable.TryChangeDamage(entity, hisGrace.DamageOnFail); // change this obvs
                    _popup.PopupEntity(Loc.GetString("hisgrace-attack-popup"), uid, PopupType.MediumCaution);
                    hisGrace.CanAttack = false;

                    break;
                }
            }

            foreach (var threshold in hisGrace.StateThreshholds.OrderByDescending(t => t.Value))
            {
                if (hisGrace.Hunger < threshold.Value)
                    continue;

                hisGrace.CurrentState = threshold.Key;
                ChangeState(hisGrace, hisGrace.CurrentState);

                break;
            }

            if (hisGrace.NextHungerTick > _timing.CurTime)
                continue;

            foreach (var increment in hisGrace.HungerIncrementThresholds.OrderByDescending(t => t.Value))
            {
                if (increment.Key == hisGrace.CurrentState)
                    hisGrace.HungerIncrement = increment.Value;
            }

            _damageable.TryChangeDamage(hisGrace.User, hisGrace.Healing);

            hisGrace.Hunger += hisGrace.HungerIncrement;
            hisGrace.NextHungerTick = _timing.CurTime + hisGrace.HungerTickDelay;
            hisGrace.CanAttack = true;
        }

    }

    private void ChangeState(HisGraceComponent comp, HisGraceState newState)
    {
        comp.CurrentState = newState;

        var ev = new HisGraceHungerChangedEvent(newState);
        RaiseLocalEvent(comp.Owner, ref ev);
    }
}
