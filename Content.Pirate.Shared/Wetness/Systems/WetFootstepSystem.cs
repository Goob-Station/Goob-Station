using Content.Pirate.Shared.Wetness.Components;
using Content.Shared._Pirate.Clothing.Events;
using Content.Shared.Inventory;

namespace Content.Pirate.Shared.Wetness.Systems;

/// <summary>
/// Makes worn wet footwear use a distinct wet step sound. Rides the same footstep relay as
/// <see cref="Content.Shared._Pirate.Clothing.Systems.EmitsSoundOnMoveSystem"/>, but instead of
/// playing its own sound it hands the sound back to the mover as the footstep override, so it plays
/// through the normal footstep path (same cadence/volume/prediction) rather than layering a second
/// sound on top of the regular step.
/// </summary>
public sealed class WetFootstepSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WetFootstepComponent, InventoryRelayedEvent<PirateMakeFootstepSoundEvent>>(OnFootstep);
    }

    private void OnFootstep(Entity<WetFootstepComponent> ent, ref InventoryRelayedEvent<PirateMakeFootstepSoundEvent> args)
    {
        if (TryComp<WettableComponent>(ent.Owner, out var wettable) && wettable.Wetness > 0)
            args.Args.OverrideSound = ent.Comp.Sound;
    }
}
