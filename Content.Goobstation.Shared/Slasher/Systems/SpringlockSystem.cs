using Content.Goobstation.Common.Slasher.Events;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._Goobstation.Clothing;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;
using System.Threading;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles spring-lock clothing that triggers when it or its wearer comes into contact with liquid.
/// </summary>
public sealed class SpringlockSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpringlockClothingComponent, InventoryRelayedEvent<SpillableCheckClothingEvent>>(OnSpillableCheckClothing);
    }

    private void OnSpillableCheckClothing(Entity<SpringlockClothingComponent> ent, ref InventoryRelayedEvent<SpillableCheckClothingEvent> args)
    {
        var method = args.Args.ReactionMethod;

        if (method != ReactionMethod.Touch || ent.Comp.IsLocked)
            return;

        ent.Comp.IsLocked = true;
        Dirty(ent);

        _appearance.SetData(ent.Owner, SpringlockVisuals.Locked, true);
        _audio.PlayPredicted(ent.Comp.LockSound, ent.Owner, ent.Owner);
    }
}
