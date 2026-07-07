using Content.Pirate.Shared.Wetness.Components;
using Content.Shared._Pirate.Clothing.Events;
using Content.Shared.Inventory;

namespace Content.Pirate.Shared.Wetness.Systems;

/// <summary>
/// Overrides footsteps for worn wet shoes.
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
