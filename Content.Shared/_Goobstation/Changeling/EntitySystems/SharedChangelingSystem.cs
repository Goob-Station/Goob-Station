using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Map;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Camera;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using System.Numerics;

namespace Content.Shared._Goobstation.Changeling.EntitySystems;

public abstract class SharedChangelingSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedStorageMapSystem _storageMap = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;
    [Dependency] protected readonly SharedStunSystem StunSystem = default!;

    public SoundSpecifier MeatSounds = new SoundCollectionSpecifier("gib");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, MobStateChangedEvent>(OnChangelingMobStateChanged);
        SubscribeLocalEvent<ChangelingComponent, DamageChangedEvent>(OnDamageChanged);

        SubscribeLocalEvent<AbsorbableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<AbsorbableComponent, MobStateChangedEvent>(OnAbsorbableMobStateChanged);
        //SubscribeLocalEvent<ChangelingComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    // TODO: MOVE THIS TO SEPARATE STRAINED MUSCLES ACTION
    /*private void OnRefreshSpeed(Entity<ChangelingComponent> changeling, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (changeling.Comp.StrainedMusclesActive)
            args.ModifySpeed(1.25f, 1.5f);
        else
            args.ModifySpeed(1f, 1f);
    }*/

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
        comp.BonusChemicalRegen = 2 * comp.MaxBonusChemicalRegen / comp.MaxBiomass
            * (comp.MaxBiomass / 2 - MathF.Abs(comp.Biomass - comp.MaxBiomass / 2));
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

    /* It should just subscribe on StingActionEvent and cancel it when something happened maaaan
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
    */

    // TODO: Make this separate system (something like HiddenItem?? dunno how to call it). Will be also useful for heretic.
    /// <summary>
    ///     Toggle and places item into ling's hand. Like armblade or meat shield.
    /// </summary>
    public bool TryToggleItem(Entity<ChangelingComponent> changeling, EntProtoId proto, string? clothingSlot = null)
    {
        var comp = changeling.Comp;

        if (!_proto.TryIndex(proto, out _))
            return false;

        // If equipment not spawned already it spawn new one and put it in hands
        if (!comp.EquipmentList.TryGetValue(proto.Id, out var item))
        {
            var spawnedEntity = Spawn(proto);
            comp.EquipmentList.Add(proto.Id, spawnedEntity);
            _hands.TryForcePickupAnyHand(changeling, spawnedEntity);

            Dirty(changeling);

            return true;
        }

        // If item have clothingSlot - try to equip it
        if (_storageMap.IsInPausedMap(item))
        {
            if (clothingSlot != null)
                return _inventory.TryEquip(changeling, item, clothingSlot, force: true);

            return _hands.TryForcePickupAnyHand(changeling, item);
        }

        _storageMap.SendToPausedStorageMap(item);
        return true;
    }

    // TODO: This really should be in different system.
    /// <summary>
    ///     Used to transfer components from one entity to another entity.
    /// </summary>
    public void TransferComponent<T>(EntityUid fromEntity, EntityUid toEntity, T component) where T : IComponent
    {
        var newComponent = _serializationManager.CreateCopy<T>(component);
        AddComp(toEntity, newComponent, true);
        Dirty(toEntity, newComponent);
        RemCompDeferred(fromEntity, component);
    }

    public bool TryTransferComponent<T>(EntityUid fromEntity, EntityUid toEntity) where T : IComponent
    {
        if (!TryComp<T>(fromEntity, out var component))
            return false;

        TransferComponent<T>(fromEntity, toEntity, component);
        return true;
    }

    /// <summary>
    ///     Changes ling's form type. Basic form type is humanoid.
    /// </summary>
    public void ChangeFormType(Entity<ChangelingComponent> changeling, ChangelingFormType formType = ChangelingFormType.HumanoidForm)
    {
        if (changeling.Comp.FormType == formType)
            return;

        changeling.Comp.FormType = formType;
        Dirty(changeling);
    }

    /// <summary>
    ///     Function to play ling's meat sound specifically 
    /// </summary>
    public void PlayMeatySound(EntityUid target)
    {
        _audio.PlayPredicted(MeatSounds, target, target);
    }

    /// <summary>
    ///     Hides all lings equipment if ling died / missclicked stasis button 
    /// </summary>
    private void OnChangelingMobStateChanged(Entity<ChangelingComponent> changeling, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        foreach (var item in changeling.Comp.EquipmentList)
        {
            _storageMap.SendToPausedStorageMap(item.Value);
        }
    }


    /// <summary>
    ///     Prevents ling to be gibbed by blunting/burning his body
    /// </summary>
    private void OnDamageChanged(Entity<ChangelingComponent> changeling, ref DamageChangedEvent args)
    {
        var target = args.Damageable;

        if (!TryComp<MobStateComponent>(changeling, out var mobState))
            return;

        if (mobState.CurrentState != MobState.Dead)
            return;

        if (!args.DamageIncreased)
            return;

        target.Damage.ClampMax(200);
    }

    /// <summary>
    ///     Removed absorbed if body somehow revived
    /// </summary>
    private void OnAbsorbableMobStateChanged(Entity<AbsorbableComponent> absorbable, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            absorbable.Comp.Absorbed = false;
    }

    /// <summary>
    ///     Adds hollowed text to text on examine if body is absorbed
    /// </summary>
    private void OnExamined(Entity<AbsorbableComponent> absorbable, ref ExaminedEvent args)
    {
        if (absorbable.Comp.Absorbed)
            args.PushMarkup(Loc.GetString("changeling-absorb-onexamine"));
    }

    /// <summary>
    ///     Change target's blood type 
    /// </summary>
    public abstract void ChangeTargetBloodType(EntityUid target, EntProtoId bloodPrototype);
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
