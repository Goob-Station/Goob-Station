using Content.Shared.Inventory;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared._Goobstation.Inventory.Events;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles spring-lock clothing that triggers when it or its wearer comes into contact with liquid.
/// </summary>
public sealed class SpringlockSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpringlockClothingComponent, InventoryRelayedEvent<ReactiveInventoryCheckEvent>>(OnReactiveInventoryCheck);
    }

    private void OnReactiveInventoryCheck(Entity<SpringlockClothingComponent> ent, ref InventoryRelayedEvent<ReactiveInventoryCheckEvent> args)
    {
        if (ent.Comp.IsLocked)
            return;

        ent.Comp.IsLocked = true;
        Dirty(ent);

        _appearance.SetData(ent.Owner, SpringlockVisuals.Locked, true);
        _audio.PlayPredicted(ent.Comp.LockSound, ent.Owner, ent.Owner);
    }
}
