using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
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
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract class SharedChangelingBiomassSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedBloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedInternalResourcesSystem _resource = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    private EntityQuery<AbsorbedComponent> _absorbQuery;
    private EntityQuery<BloodstreamComponent> _bloodQuery;
    private EntityQuery<ChangelingIdentityComponent> _lingQuery;
    private EntityQuery<InternalResourcesComponent> _resourceQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingBiomassComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChangelingBiomassComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ChangelingBiomassComponent, InternalResourcesThresholdMetEvent>(OnThresholdMetEvent);
        SubscribeLocalEvent<ChangelingBiomassComponent, InternalResourcesRegenModifierEvent>(OnChangelingChemicalRegenEvent);
        SubscribeLocalEvent<ChangelingBiomassComponent, RejuvenateEvent>(OnRejuvenate);

        _absorbQuery = GetEntityQuery<AbsorbedComponent>();
        _bloodQuery = GetEntityQuery<BloodstreamComponent>();
        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
        _resourceQuery = GetEntityQuery<InternalResourcesComponent>();
    }

    private void OnMapInit(Entity<ChangelingBiomassComponent> ent, ref MapInitEvent args)
    {
        _resource.TryAddInternalResources(ent, ent.Comp.ResourceProto, out var data);

        if (data != null)
            ent.Comp.ResourceData = data;
    }

    private void OnShutdown(Entity<ChangelingBiomassComponent> ent, ref ComponentShutdown args)
    {
        if (_resourceQuery.TryComp(ent, out var resComp))
            _resource.TryRemoveInternalResource(ent, ent.Comp.ResourceProto, resComp);
    }

    #region Event Handlers

    private void OnThresholdMetEvent(Entity<ChangelingBiomassComponent> ent, ref InternalResourcesThresholdMetEvent args)
    {
        if (ent.Comp.ResourceData == null
            || args.Data.InternalResourcesType != ent.Comp.ResourceData.InternalResourcesType)
            return;

        switch (args.Threshold)
        {
            case "First":

                DoPopup(ent, ent.Comp.FirstWarnPopup, PopupType.SmallCaution);

                break;

            case "Second":

                _stun.TryStun(ent, ent.Comp.SecondWarnStun, false);
                DoPopup(ent, ent.Comp.SecondWarnPopup, PopupType.MediumCaution);

                break;

            case "Third":

                _stun.TryStun(ent, ent.Comp.ThirdWarnStun, false);

                if (!_blood.TryModifyBloodLevel(ent.Owner, -ent.Comp.BloodCoughAmount)
                    || !_bloodQuery.TryComp(ent, out var bloodComp))
                {
                    _stun.TryKnockdown(ent, ent.Comp.ThirdWarnStun, false); // knockdown if there isnt any blood to cough up
                    return;
                }

                var cough = new Solution();
                cough.AddReagent(bloodComp.BloodReagent, ent.Comp.BloodCoughAmount);

                _puddle.TrySpillAt(Transform(ent).Coordinates, cough, out _, false);

                if (!_mob.IsDead(ent)) // i guess you... drool it out otherwise
                    DoCough(ent);

                DoPopup(ent, ent.Comp.ThirdWarnPopup, PopupType.LargeCaution);

                break;

            case "Death":

                if (!_absorbQuery.HasComp(ent))
                    KillChangeling(ent);

                DoPopup(ent, ent.Comp.NoBiomassPopup, PopupType.LargeCaution);

                break;

            default:
                return;
        }
    }

    private void OnChangelingChemicalRegenEvent(Entity<ChangelingBiomassComponent> ent, ref InternalResourcesRegenModifierEvent args)
    {
        if (args.Data.InternalResourcesType != ent.Comp.ChemResourceType)
            return;

        args.Modifier += ent.Comp.ChemicalBoost;
    }

    private void OnRejuvenate(Entity<ChangelingBiomassComponent> ent, ref RejuvenateEvent args)
    {
        if (ent.Comp.ResourceData == null
            || _lingQuery.TryComp(ent, out var ling)
            && ling.IsInStasis)
            return;

        _resource.TryUpdateResourcesAmount(ent, ent.Comp.ResourceData, ent.Comp.ResourceData.MaxAmount);
    }

    #endregion

    #region Helper Methods

    public readonly ProtoId<DamageTypePrototype> Genetic = "Cellular";
    private void KillChangeling(Entity<ChangelingBiomassComponent> ent)
    {
        var genetic = _proto.Index(Genetic);

        if (!_mobThreshold.TryGetDeadThreshold(ent, out var totalDamage))
            return;

        var damagespec = new DamageSpecifier(genetic, (FixedPoint2) totalDamage);
        _dmg.TryChangeDamage(ent, damagespec, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);

        EnsureComp<AbsorbedComponent>(ent);
    }

    protected virtual void DoCough(Entity<ChangelingBiomassComponent> ent)
    {
        // go to ChangelingBiomassSystem for the logic
    }

    private void DoPopup(Entity<ChangelingBiomassComponent> ent, LocId popup, PopupType popupType)
    {
        _popup.PopupEntity(Loc.GetString(popup), ent, ent, popupType);
    }

    #endregion
}
