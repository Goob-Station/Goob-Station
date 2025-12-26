using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.InternalResources.Data;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Fluids;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Stunnable;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract class SharedChangelingBiomassSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly MobThresholdSystem _mob = default!;
    [Dependency] private readonly SharedBloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    private EntityQuery<AbsorbedComponent> _absorbQuery;
    private EntityQuery<BloodstreamComponent> _bloodQuery;

    public override void Initialize()
    {
        base.Initialize();

        _absorbQuery = GetEntityQuery<AbsorbedComponent>();
        _bloodQuery = GetEntityQuery<BloodstreamComponent>();

        SubscribeLocalEvent<ChangelingBiomassComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChangelingBiomassComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ChangelingBiomassComponent, ChangelingModifyBiomassEvent>(OnModifyBiomassEvent);
        SubscribeLocalEvent<ChangelingBiomassComponent, InternalResourcesRegenModifierEvent>(OnChangelingChemicalRegenEvent);
        SubscribeLocalEvent<ChangelingBiomassComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnMapInit(Entity<ChangelingBiomassComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        ent.Comp.FirstWarnThreshold = ent.Comp.MaxBiomass * 0.75f;
        ent.Comp.SecondWarnThreshold = ent.Comp.MaxBiomass * 0.5f;
        ent.Comp.ThirdWarnThreshold = ent.Comp.MaxBiomass * 0.25f;

        Dirty(ent);

        Cycle(ent);
    }

    private void OnShutdown(Entity<ChangelingBiomassComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent, ent.Comp.AlertId);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChangelingBiomassComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;
            Dirty(uid, comp);

            Cycle((uid, comp));
        }
    }

    private void Cycle(Entity<ChangelingBiomassComponent> ent)
    {
        UpdateBiomass(ent, -ent.Comp.DrainAmount);

        // first
        if (!ent.Comp.FirstWarnReached
            && ent.Comp.Biomass <= ent.Comp.FirstWarnThreshold)
        {
            ent.Comp.FirstWarnReached = true;

            DoPopup(ent, ent.Comp.FirstWarnPopup, PopupType.SmallCaution);
        }
        else if (ent.Comp.Biomass > ent.Comp.FirstWarnThreshold)
            ent.Comp.FirstWarnReached = false;

        DirtyField(ent, ent.Comp, nameof(ChangelingBiomassComponent.FirstWarnReached));

        // second
        if (!ent.Comp.SecondWarnReached
            && ent.Comp.Biomass <= ent.Comp.SecondWarnThreshold)
        {
            ent.Comp.SecondWarnReached = true;

            DoPopup(ent, ent.Comp.SecondWarnPopup, PopupType.MediumCaution);

            _stun.TryStun(ent, ent.Comp.SecondWarnStun, false);
        }
        else if (ent.Comp.Biomass > ent.Comp.SecondWarnThreshold)
            ent.Comp.SecondWarnReached = false;

        DirtyField(ent, ent.Comp, nameof(ChangelingBiomassComponent.SecondWarnReached));

        // third
        if (!ent.Comp.ThirdWarnReached
            && ent.Comp.Biomass <= ent.Comp.ThirdWarnThreshold)
        {
            ent.Comp.ThirdWarnReached = true;

            DoPopup(ent, ent.Comp.ThirdWarnPopup, PopupType.LargeCaution);

            _stun.TryStun(ent, ent.Comp.ThirdWarnStun, false);

            // do the blood cough
            if (!_blood.TryModifyBloodLevel(ent.Owner, -ent.Comp.BloodCoughAmount)
                || !_bloodQuery.TryComp(ent, out var bloodComp))
            {
                _stun.TryKnockdown(ent, ent.Comp.ThirdWarnStun, false);
                return;
            }

            var cough = new Solution();
            cough.AddReagent(bloodComp.BloodReagent, ent.Comp.BloodCoughAmount);

            _puddle.TrySpillAt(Transform(ent).Coordinates, cough, out _, false);
            DoCough(ent);

        }
        else if (ent.Comp.Biomass > ent.Comp.ThirdWarnThreshold)
            ent.Comp.ThirdWarnReached = false;

        DirtyField(ent, ent.Comp, nameof(ChangelingBiomassComponent.ThirdWarnReached));

        // point of no return
        if (ent.Comp.Biomass <= 0
            && !_absorbQuery.HasComp(ent))
            KillChangeling(ent);

    }

    #region Helper Methods
    private void UpdateBiomass(Entity<ChangelingBiomassComponent> ent, float? amount = null)
    {
        var newBiomass = ent.Comp.Biomass;

        newBiomass += amount ?? 0;
        ent.Comp.Biomass = Math.Clamp(newBiomass, 0, ent.Comp.MaxBiomass);

        _alerts.ShowAlert(ent, ent.Comp.AlertId);

        Dirty(ent);
    }

    public readonly ProtoId<DamageTypePrototype> Genetic = "Cellular";
    private void KillChangeling(Entity<ChangelingBiomassComponent> ent)
    {
        var genetic = _proto.Index(Genetic);

        if (!_mob.TryGetDeadThreshold(ent, out var totalDamage))
            return;

        var damagespec = new DamageSpecifier(genetic, (FixedPoint2) totalDamage);
        _dmg.TryChangeDamage(ent, damagespec, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);

        EnsureComp<AbsorbedComponent>(ent);

        DoPopup(ent, ent.Comp.NoBiomassPopup, PopupType.LargeCaution);
    }

    protected virtual void DoCough(Entity<ChangelingBiomassComponent> ent)
    {
        // go to ChangelingBiomassSystem for the logic
    }

    private void DoPopup(Entity<ChangelingBiomassComponent> ent, LocId popup, PopupType popupType)
    {
        if (_net.IsClient)
            return;

        _popup.PopupEntity(Loc.GetString(popup), ent, ent, popupType);
    }

    #endregion

    #region Event Handlers
    private void OnModifyBiomassEvent(Entity<ChangelingBiomassComponent> ent, ref ChangelingModifyBiomassEvent args)
    {
        UpdateBiomass(ent, args.Amount);
    }

    private void OnChangelingChemicalRegenEvent(Entity<ChangelingBiomassComponent> ent, ref InternalResourcesRegenModifierEvent args)
    {
        if (args.Data.InternalResourcesType != ent.Comp.ResourceType)
            return;

        args.Modifier += ent.Comp.ChemicalBoost;
    }

    private void OnRejuvenate(Entity<ChangelingBiomassComponent> ent, ref RejuvenateEvent args)
    {
        UpdateBiomass(ent, ent.Comp.MaxBiomass);
    }

    #endregion
}
