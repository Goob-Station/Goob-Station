using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;

namespace Content.Shared.Clothing;

/// <summary>
/// Changes the friction and acceleration of the wearer when wearing wizard skates,
/// </summary>
public sealed class WizardSkatesSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _move = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardSkatesComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<WizardSkatesComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
    }

    /// <summary>
    /// When item is unequipped from the shoe slot, friction and acceleration return to default settings.
    /// </summary>
    public void OnGotUnequipped(EntityUid uid, WizardSkatesComponent component, ClothingGotUnequippedEvent args)
    {
        if (!TryComp(args.Wearer, out MovementSpeedModifierComponent? speedModifier))
            return;

        // Revert friction and acceleration to the default values when unequipping.
        _move.ChangeFriction(args.Wearer, MovementSpeedModifierComponent.DefaultFriction, MovementSpeedModifierComponent.DefaultFrictionNoInput, MovementSpeedModifierComponent.DefaultAcceleration, speedModifier);
    }

    /// <summary>
    /// When item is equipped into the shoe slot, friction and acceleration are adjusted for the wizard skates.
    /// </summary>
    private void OnGotEquipped(EntityUid uid, WizardSkatesComponent component, ClothingGotEquippedEvent args)
    {
        _move.ChangeFriction(args.Wearer, component.Friction, component.FrictionNoInput, component.Acceleration);
    }
}