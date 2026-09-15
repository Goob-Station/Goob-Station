// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Oskarrr.WildLandsTribe;

public sealed class TribePsiSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly ProtoId<TagPrototype> AborigineTag = "WildLandsAborigine";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TribePsiComponent, AboriginePsiHealEvent>(OnPsiHeal);
        SubscribeLocalEvent<TribePsiComponent, AboriginePsiRegenEvent>(OnPsiRegen);
        SubscribeLocalEvent<TribePsiComponent, AboriginePsiResurrectEvent>(OnPsiResurrect);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        UpdateEnergy(frameTime);
        UpdateRegenBuffs(frameTime);
    }

    private void UpdateEnergy(float frameTime)
    {
        var query = EntityQueryEnumerator<TribePsiComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var psi, out var xform))
        {
            if (IsOnInstantRegenTile(xform, psi))
            {
                if (psi.Energy < psi.MaxEnergy)
                {
                    psi.Energy = psi.MaxEnergy;
                    Dirty(uid, psi);
                }
                continue;
            }

            if (psi.Energy >= psi.MaxEnergy)
                continue;

            psi.Accumulator += frameTime;
            if (psi.Accumulator < 1f)
                continue;

            psi.Accumulator = 0f;
            psi.Energy = MathF.Min(psi.MaxEnergy, psi.Energy + psi.RegenPerSecond);
            Dirty(uid, psi);
        }
    }

    private void UpdateRegenBuffs(float frameTime)
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<TribePsiRegenBuffComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var buff, out _))
        {
            if (now >= buff.EndTime)
            {
                RemCompDeferred<TribePsiRegenBuffComponent>(uid);
                continue;
            }

            if (_mobState.IsDead(uid))
                continue;

            buff.Accumulator += frameTime;
            if (buff.Accumulator < buff.TickInterval)
                continue;

            buff.Accumulator = 0f;
            _damageable.TryChangeDamage(uid, buff.HealPerTick, true, origin: uid);
        }
    }

    private bool IsOnInstantRegenTile(TransformComponent xform, TribePsiComponent psi)
    {
        if (!_turf.TryGetTileRef(xform.Coordinates, out var tileRef))
            return false;

        var tileId = _tileDef[tileRef.Value.Tile.TypeId].ID;
        return psi.InstantRegenTiles.Contains(tileId);
    }

    private bool TrySpendEnergy(Entity<TribePsiComponent> ent, float cost)
    {
        if (ent.Comp.Energy < cost)
        {
            _popup.PopupClient(Loc.GetString("aborigine-psi-no-energy"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return false;
        }

        ent.Comp.Energy -= cost;
        Dirty(ent);
        return true;
    }

    private bool IsTribeAlly(EntityUid caster, EntityUid target)
    {
        return target == caster || _tag.HasTag(target, AborigineTag);
    }

    private void OnPsiHeal(Entity<TribePsiComponent> ent, ref AboriginePsiHealEvent args)
    {
        if (args.Handled)
            return;

        if (!TrySpendEnergy(ent, ent.Comp.HealCost))
            return;

        var healed = 0;
        var targets = new HashSet<EntityUid>();
        _lookup.GetEntitiesInRange(_transform.GetMoverCoordinates(ent.Owner), ent.Comp.HealRange, targets);

        foreach (var target in targets)
        {
            if (!HasComp<MobStateComponent>(target) || !HasComp<DamageableComponent>(target))
                continue;

            if (!IsTribeAlly(ent.Owner, target))
                continue;

            if (_mobState.IsDead(target))
                continue;

            _damageable.TryChangeDamage(target, ent.Comp.HealDamage, true, origin: ent.Owner);
            healed++;
        }

        _popup.PopupClient(
            Loc.GetString("aborigine-psi-heal", ("count", healed)),
            ent.Owner,
            ent.Owner,
            PopupType.Medium);

        args.Handled = true;
    }

    private void OnPsiRegen(Entity<TribePsiComponent> ent, ref AboriginePsiRegenEvent args)
    {
        if (args.Handled)
            return;

        if (!TrySpendEnergy(ent, ent.Comp.RegenCost))
            return;

        var buffed = 0;
        var targets = new HashSet<EntityUid>();
        _lookup.GetEntitiesInRange(_transform.GetMoverCoordinates(ent.Owner), ent.Comp.RegenRange, targets);

        var end = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.RegenDuration);

        foreach (var target in targets)
        {
            if (!HasComp<MobStateComponent>(target) || !HasComp<DamageableComponent>(target))
                continue;

            if (!IsTribeAlly(ent.Owner, target))
                continue;

            if (_mobState.IsDead(target))
                continue;

            var buff = EnsureComp<TribePsiRegenBuffComponent>(target);
            buff.EndTime = end;
            buff.TickInterval = ent.Comp.RegenTickInterval;
            buff.HealPerTick = ent.Comp.RegenDamage;
            buff.Accumulator = 0f;
            Dirty(target, buff);
            buffed++;
        }

        _popup.PopupClient(
            Loc.GetString("aborigine-psi-regen", ("count", buffed)),
            ent.Owner,
            ent.Owner,
            PopupType.Medium);

        args.Handled = true;
    }

    private void OnPsiResurrect(Entity<TribePsiComponent> ent, ref AboriginePsiResurrectEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!IsTribeAlly(ent.Owner, target))
        {
            _popup.PopupClient(Loc.GetString("aborigine-psi-resurrect-not-tribe"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _popup.PopupClient(Loc.GetString("aborigine-psi-resurrect-not-dead"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        if (HasComp<UnrevivableComponent>(target))
        {
            _popup.PopupClient(Loc.GetString("aborigine-psi-resurrect-fail"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        var distance = (_transform.GetWorldPosition(ent.Owner) - _transform.GetWorldPosition(target)).Length();
        if (distance > ent.Comp.ResurrectRange)
        {
            _popup.PopupClient(Loc.GetString("aborigine-psi-resurrect-far"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        if (!TrySpendEnergy(ent, ent.Comp.ResurrectCost))
            return;

        _mobThreshold.SetAllowRevives(target, true);
        _damageable.TryChangeDamage(target, ent.Comp.ResurrectHeal, true, origin: ent.Owner);

        if (TryComp<MobStateComponent>(target, out var mobState))
            _mobState.ChangeMobState(target, MobState.Critical, mobState, ent.Owner);

        _mobThreshold.SetAllowRevives(target, false);

        // Keep them alive-ish: apply another heal burst and push to Alive if possible
        _damageable.TryChangeDamage(target, ent.Comp.HealDamage, true, origin: ent.Owner);
        if (TryComp(target, out mobState) && !_mobState.IsDead(target, mobState))
            _mobState.ChangeMobState(target, MobState.Alive, mobState, ent.Owner);

        _popup.PopupClient(Loc.GetString("aborigine-psi-resurrect"), ent.Owner, ent.Owner, PopupType.Medium);
        _popup.PopupEntity(Loc.GetString("aborigine-psi-resurrect-target"), target, PopupType.Medium);

        args.Handled = true;
    }
}
