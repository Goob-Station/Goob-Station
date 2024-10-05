using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Camera;
using Content.Shared.Changeling;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.IdentityManagement;
using Content.Shared.Jittering;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Shared._Goobstation.Changeling;

public abstract class SharedChangelingSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;
    [Dependency] protected readonly SharedStunSystem StunSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    // TODO: MOVE THIS TO SEPARATE STRAINED MUSCLES ACTION
    private void OnRefreshSpeed(Entity<ChangelingComponent> changeling, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (changeling.Comp.StrainedMusclesActive)
            args.ModifySpeed(1.25f, 1.5f);
        else
            args.ModifySpeed(1f, 1f);
    }

    public bool TryUseChangelingAbility(Entity<ChangelingComponent> changeling, BaseActionEvent action)
    {
        if (action.Handled)
            return false;

        var comp = changeling.Comp;

        if (!TryComp<ChangelingActionComponent>(action.Action, out var lingAction))
            return false;

        if (comp.Biomass < lingAction.BiomassCost)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-biomass-deficit"), changeling);
            return false;
        }

        if (comp.FormType < lingAction.RequiredFormType)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-action-fail-lesserform"), changeling);
            return false;
        }

        if (comp.Chemicals < lingAction.ChemicalCost)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-chemicals-deficit"), changeling);
            return false;
        }

        if (comp.TotalAbsorbedEntities < lingAction.RequireAbsorbed) 
        {
            var delta = lingAction.RequireAbsorbed - comp.TotalAbsorbedEntities;
            PopupSystem.PopupClient(Loc.GetString("changeling-action-fail-absorbed", ("number", delta)), changeling);
            return false;
        }

        UpdateChemicals(changeling, -lingAction.ChemicalCost);
        UpdateBiomass(changeling, -lingAction.BiomassCost);

        action.Handled = true;

        return true;
    }

    /// <summary>
    ///     Updates chemicals amount and updates client alert sprite
    /// </summary>
    public void UpdateChemicals(Entity<ChangelingComponent> changeling, float? amount = null)
    {
        var comp = changeling.Comp;

        var chemicals = comp.Chemicals;
        chemicals += amount ?? 1 + comp.BonusChemicalRegen;
        comp.Chemicals = Math.Clamp(chemicals, 0, comp.MaxChemicals);

        Dirty(changeling);

        _alerts.ShowAlert(changeling, comp.ChemicalsAlert);
    }

    /// <summary>
    ///     Updates biomass amount, updates client alert sprite and applys jittering/chemicalBonus depending on biomass count
    /// </summary>
    public virtual void UpdateBiomass(Entity<ChangelingComponent> changeling, float? amount = null)
    {
        var comp = changeling.Comp;

        comp.Biomass += amount ?? -1;
        comp.Biomass = Math.Clamp(comp.Biomass, 0, comp.MaxBiomass);
        comp.BonusChemicalRegen = -4 * comp.MaxBonusChemicalRegen / MathF.Pow(comp.MaxBiomass, 2)
            * MathF.Pow(comp.Biomass - comp.MaxBiomass / 2, 2) + comp.MaxBonusChemicalRegen;
        // ↑ This formula makes maximumChemicalRegen on the half between 0 and MaxBiomass.
        // So, ling will be more powerful when he have half of biomass, because he don't want to eat on all biomass and will be weak on 0 biomass

        _alerts.ShowAlert(changeling, comp.BiomassAlert);

        // Deprive ability to use stasis for ling AND disables biomass update for perfomance.
        // This will also allow peaceful lings, because they'll stop jittering on 0 biomass.
        if (comp.Biomass <= 0)
            comp.IsEmptyBiomass = true;

        Dirty(changeling);
    }

    /// <summary>
    ///     Playing loudsound making client's camera to 
    /// </summary>
    public void DoScreech(Entity<ChangelingComponent> changeling, float radius = 2.5f)
    {
        var comp = changeling.Comp;

        _audio.PlayPredicted(comp.ShriekSound, changeling, changeling);

        var lingCoordinates = _transform.GetMapCoordinates(changeling);
        var gamers = Filter.Empty().AddInRange(lingCoordinates, radius);

        foreach (var gamer in gamers.Recipients)
        {
            if (gamer.AttachedEntity == null)
                continue;

            var gamerPosition = _transform.GetWorldPosition(gamer.AttachedEntity.Value);
            var delta = lingCoordinates.Position - gamerPosition;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(.01f, 0);

            _recoil.KickCamera(changeling, -delta.Normalized());
        }
    }

    /// <summary>
    ///     Tries to sting someone by action
    /// </summary>
    /// <returns></returns>
    public bool TrySting(Entity<ChangelingComponent> changeling, EntityTargetActionEvent action)
    {
        if (!TryUseChangelingAbility(changeling, action))
            return false;

        var target = action.Target;

        // Can't get DNA from body which DNA was absorbed already
        if (!TryComp<AbsorbableComponent>(target, out var absorbable) || absorbable.Absorbed)
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-sting-extract-fail"), changeling);
            return false;
        }

        // You can't sting another ling
        if (HasComp<ChangelingComponent>(target))
        {
            PopupSystem.PopupClient(Loc.GetString("changeling-sting-fail-self", ("target", Identity.Entity(target, EntityManager))), changeling);
            PopupSystem.PopupClient(Loc.GetString("changeling-sting-fail-ling"), target, target);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Adds reagents in target's injectable solution
    /// </summary>
    public bool TryInjectReagents(EntityUid target, List<(string, FixedPoint2)> reagents)
    {
        var solution = new Solution();

        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Item1, reagent.Item2);

        if (!_solution.TryGetInjectableSolution(target, out var targetSolution, out var _))
            return false;

        if (!_solution.TryAddSolution(targetSolution.Value, solution))
            return false;

        return true;
    }

    /// <summary>
    ///     Combination of TrySting and TryInjectReagents
    /// </summary>
    public bool TryReagentSting(Entity<ChangelingComponent> changeling, EntityTargetActionEvent action, List<(string, FixedPoint2)> reagents)
    {
        var target = action.Target;
        if (!TrySting(changeling, action))
            return false;

        if (!TryInjectReagents(target, reagents))
            return false;

        return true;
    }

    /// <summary>
    ///     Toggles item in ling's hand. Like armblade or meat shield.
    /// </summary>
    public bool TryToggleItem(Entity<ChangelingComponent> changeling, EntProtoId proto, string? clothingSlot = null)
    {
        if (!comp.Equipment.TryGetValue(proto.Id, out var item) && item == null)
        {
            item = Spawn(proto, Transform(uid).Coordinates);
            if (clothingSlot != null && !_inventory.TryEquip(uid, (EntityUid) item, clothingSlot, force: true))
            {
                QueueDel(item);
                return false;
            }
            else if (!_hands.TryForcePickupAnyHand(uid, (EntityUid) item))
            {
                _popup.PopupEntity(Loc.GetString("changeling-fail-hands"), uid, uid);
                QueueDel(item);
                return false;
            }
            comp.Equipment.Add(proto.Id, item);
            return true;
        }

        QueueDel(item);
        // assuming that it exists
        comp.Equipment.Remove(proto.Id);

        return true;
    }
}

[Serializable, NetSerializable]
public enum ChangelingFormType : byte
{
    /// <summary>
    ///     Lowest form possible. Use if you want to make ling useless or to make ling ability without any form requirment (stasis have own form).
    /// </summary>
    NoForm,
    /// <summary>
    ///     Ling in any form, but in stasis. Use on ability, if you want to make it activateable in stasis.
    /// </summary>
    StasisForm,
    /// <summary>
    ///     Ling in slug form. Use if you want ling to activate ability in form of mouse/slug or any small mob.
    /// </summary>
    SlugForm,
    /// <summary>
    ///     Ling in monkey form. Use if you want ling to activate ability in form of monkey/another animal form.
    /// </summary>
    LesserForm,
    /// <summary>
    ///     Ling in humanoid form. Use if you want ling to activate ability in form of any humanoid race.
    /// </summary>
    HumanoidForm,
}
