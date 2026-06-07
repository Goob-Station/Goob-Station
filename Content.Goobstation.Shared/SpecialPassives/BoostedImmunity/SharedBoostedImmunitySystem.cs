using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Medical;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Alert;
using Content.Shared.Body.Systems;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Drunk;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public abstract class SharedBoostedImmunitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly BlindableSystem _blindSys = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodSys = default!;
    [Dependency] private readonly SharedDrunkSystem _drunkSys = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private EntityQuery<DamageableComponent> _damageableQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;
    private EntityQuery<StatusEffectsComponent> _statusQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        _statusQuery = GetEntityQuery<StatusEffectsComponent>();

        SubscribeLocalEvent<BoostedImmunityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BoostedImmunityComponent, ComponentRemove>(OnRemoved);

        SubscribeLocalEvent<BoostedImmunityComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<BoostedImmunityComponent, BeforeVomitEvent>(OnBeforeVomitEvent);
    }

    private void OnMapInit(Entity<BoostedImmunityComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        if (_mobStateQuery.TryComp(ent, out var state))
            ent.Comp.Mobstate = state.CurrentState;

        if (ent.Comp.RemoveDisabilities)
            RemoveDisabilities(ent);

        if (ent.Comp.RemoveAlienEmbryo)
            RemoveAlienEmbryo(ent);

        if (ent.Comp.RemoveDiseases)
            RemoveDiseases(ent);

        Cycle(ent);
    }

    private void OnRemoved(Entity<BoostedImmunityComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.AlertId != null)
            _alerts.ClearAlert(ent, (ProtoId<AlertPrototype>) ent.Comp.AlertId); // incase there was still time left on removal
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<BoostedImmunityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.MaxDuration < _timing.CurTime
                && comp.Duration.HasValue) // assume it lasts forever otherwise
                RemCompDeferred<BoostedImmunityComponent>(uid);

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    private void Cycle(Entity<BoostedImmunityComponent> ent)
    {
        if (!TryValidFireCheck(ent)
            || !TryValidMobstateCheck(ent))
            return;

        if (_statusQuery.TryComp(ent, out var status))
        {
            if (ent.Comp.ApplySober)
                SoberEntity(ent, status);

            if (ent.Comp.RemovePacifism)
                RemovePacifism(ent, status);
        }

        if (ent.Comp.CleanseChemicals)
            _bloodSys.FlushChemicals(ent.Owner, null, ent.Comp.CleanseChemicalsAmount);

        HealDamage(ent);
    }

    #region Event Handlers
    private void OnMobStateChange(Entity<BoostedImmunityComponent> ent, ref MobStateChangedEvent args)
    {
        ent.Comp.Mobstate = args.NewMobState;
    }

    private void OnBeforeVomitEvent(Entity<BoostedImmunityComponent> ent, ref BeforeVomitEvent args)
    {
        args.Cancelled = ent.Comp.ResistNausea;
    }
    #endregion

    #region Helper Methods

    public readonly ProtoId<DamageGroupPrototype> ToxinDamageGroup = "Toxin";
    private void HealDamage(Entity<BoostedImmunityComponent> ent)
    {
        // the dmg groups
        var toxinTypes = _proto.Index(ToxinDamageGroup);

        // set the amount of healing depending on non-zero damage types present
        if (!_damageableQuery.TryComp(ent, out var damage))
            return;

        var toxinDiv =
            toxinTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type)
            != FixedPoint2.Zero);

        var toxinHealAmount = ent.Comp.ToxinHeal / toxinDiv;

        var healSpec = new DamageSpecifier();

        foreach (var tox in toxinTypes.DamageTypes)
            healSpec.DamageDict.Add(tox, toxinHealAmount);

        healSpec.DamageDict.Add("Cellular", ent.Comp.CellularHeal);

        // heal the damage
        _dmg.TryChangeDamage(ent, healSpec, true, false, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);
        HealEyes(ent);
    }

    private void HealEyes(Entity<BoostedImmunityComponent> ent)
    {
        _blindSys.AdjustEyeDamage(ent.Owner, -ent.Comp.EyeDamageHeal);

        if (!_statusQuery.TryComp(ent, out var status))
            return;

        _status.TryRemoveStatusEffect(ent, "TemporaryBlindness", status);
        _status.TryRemoveStatusEffect(ent, "BlurryVision", status);
    }

    private void SoberEntity(Entity<BoostedImmunityComponent> ent, StatusEffectsComponent status)
    {
        _drunkSys.TryRemoveDrunkenness(ent);
        _status.TryRemoveStatusEffect(ent, "SeeingRainbows", status);
    }

    private void RemovePacifism(Entity<BoostedImmunityComponent> ent, StatusEffectsComponent status)
    {
        _status.TryRemoveStatusEffect(ent, "Pacified", status);
        RemComp<PacifiedComponent>(ent); // might not be tied to a status effect
    }

    private bool TryValidFireCheck(Entity<BoostedImmunityComponent> ent)
    {
        var fireEv = new GetFireStateEvent();
        RaiseLocalEvent(ent, ref fireEv);

        if (fireEv.OnFire
            && !ent.Comp.IgnoreFire)
            return false;

        return true;
    }

    private bool TryValidMobstateCheck(Entity<BoostedImmunityComponent> ent)
    {
        if (ent.Comp.Mobstate == MobState.Dead
            && !ent.Comp.WorkWhileDead)
        {
            RemComp<BoostedImmunityComponent>(ent);
            return false;
        }

        return true;
    }

    protected virtual void RemoveDisabilities(Entity<BoostedImmunityComponent> ent)
    {
        // go to BoostedImmunitySystem for the logic
    }

    protected virtual void RemoveAlienEmbryo(Entity<BoostedImmunityComponent> ent)
    {
        // go to BoostedImmunitySystem for the logic
    }

    protected virtual void RemoveDiseases(Entity<BoostedImmunityComponent> ent)
    {
        // go to BoostedImmunitySystem for the logic
    }

    #endregion
}
