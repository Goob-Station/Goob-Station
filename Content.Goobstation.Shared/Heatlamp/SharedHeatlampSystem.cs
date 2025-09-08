using Content.Shared.Damage;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Heatlamp;

// TODO: euaghbhdh reorganize
public abstract class SharedHeatlampSystem : EntitySystem
{
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatlampComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<HeatlampComponent, ItemToggledEvent>(OnItemToggled);
        SubscribeLocalEvent<HeatlampComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnComponentInit(Entity<HeatlampComponent> ent, ref ComponentInit args)
    {
        // Set up dynamic values
        UpdateDynamicValues(ent);

        // Apply dynamic values. Shitcode, Test frequently.
        ApplyDynamicValues(ent);
    }

    private void OnEmagged(Entity<HeatlampComponent> ent, ref GotEmaggedEvent args)
    {
        // Check that we're using an emag
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        // Check that we aren't already emagged
        if (HasComp<EmaggedComponent>(ent))
            return;

        // Update our appearance
        _appearance.SetData(ent, HeatlampVisuals.IsEmagged, true);

        // Tell EmagSystem to use the charge
        args.Handled = true;
    }

    private void OnItemToggled(Entity<HeatlampComponent> ent, ref ItemToggledEvent args)
    {
        // Update our damage if we're on a weapon
        MaybeUpdateDamage(ent);
    }

    /// <summary>
    ///     Updates the modified values for ent, does not apply them to other components.
    /// </summary>
    private void UpdateDynamicValues(Entity<HeatlampComponent> ent)
    {
        ent.Comp.ModifiedHeatingPowerDrain = ent.Comp.BaseHeatingPowerDrain;
        ent.Comp.ModifiedMaximumHeatingPerUpdate = ent.Comp.BaseMaximumHeatingPerUpdate;
        ent.Comp.ModifiedCoolingPowerDrain = ent.Comp.BaseCoolingPowerDrain;
        ent.Comp.ModifiedMaximumCoolingPerUpdate = ent.Comp.BaseMaximumCoolingPerUpdate;
        ent.Comp.ModifiedActivatedDamage = new DamageSpecifier(ent.Comp.BaseActivatedDamage);
    }

    /// <summary>
    ///     Applies modified values to other components.
    /// </summary>
    private void ApplyDynamicValues(Entity<HeatlampComponent> ent)
    {
        MaybeUpdateDamage(ent);
    }

    /// <summary>
    ///     Updates damage if attached to a weapon.
    /// </summary>
    private void MaybeUpdateDamage(Entity<HeatlampComponent> ent)
    {
        // Set damage if we're attached to a weapon
        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            melee.Damage = new DamageSpecifier(IsActive(ent) ? ent.Comp.ModifiedActivatedDamage : ent.Comp.DeactivatedDamage);
        }
    }

    private bool IsActive(Entity<HeatlampComponent> ent)
    {
        // ReSharper disable once SimplifyConditionalTernaryExpression // that looks like shit
        // Entities with the heatlamp component are not always heatlamps,
        // and will not always have ItemToggleComponents. Namely, the Thermal Regulator
        // organ adds a heatlamp component to it's user.
        return TryComp<ItemToggleComponent>(ent, out var toggle) ? toggle.Activated : true;
    }
}

[Serializable, NetSerializable]
public enum HeatlampVisuals : byte
{
    IsPowered,
    IsEmagged
}
