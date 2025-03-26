using Content.Goobstation.Shared.Flashbang;
using Content.Shared._White.Overlays;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.SwitchableOverlay;

public sealed class GoobSwitchableOverlaySystem<T> : EntitySystem where T : SwitchableVisionOverlayComponent
{
    public override void Initialize()
    {
        SubscribeLocalEvent<T, FlashDurationMultiplierEvent>(OnGetFlashMultiplier);
        SubscribeLocalEvent<T, InventoryRelayedEvent<FlashDurationMultiplierEvent>>(OnGetInventoryFlashMultiplier);
    }

    private void OnGetFlashMultiplier(Entity<T> ent, ref FlashDurationMultiplierEvent args)
    {
        if (!ent.Comp.IsEquipment)
            args.Multiplier *= GetFlashMultiplier(ent);
    }

    private void OnGetInventoryFlashMultiplier(Entity<T> ent, ref InventoryRelayedEvent<FlashDurationMultiplierEvent> args)
    {
        if (ent.Comp.IsEquipment)
            args.Args.Multiplier *= GetFlashMultiplier(ent);
    }

    private float GetFlashMultiplier(T comp)
    {
        if (!comp.IsActive && (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime))
            return 1f;

        return comp.FlashDurationMultiplier;
    }
}
