using Content.Goobstation.Common.Slasher.Events;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Inventory;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Shared.Audio.Systems;

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

        SubscribeLocalEvent<ReactiveComponent, ShitRelayEventFixMeReactionEntityEvent>(OnReactionEntity);
    }

    private void OnReactionEntity(Entity<ReactiveComponent> ent, ref ShitRelayEventFixMeReactionEntityEvent args)
    {
        if (
            //args.Method != ReactionMethod.Touch ||
            !HasComp<InventoryComponent>(ent.Owner))
            return;

        var slots = _inventory.GetSlotEnumerator(ent.Owner, SlotFlags.WITHOUT_POCKET);
        while (slots.NextItem(out var item))
        {
            if (!TryComp<SpringlockClothingComponent>(item, out var springlock) || springlock.IsLocked)
                continue;

            springlock.IsLocked = true;
            Dirty(item, springlock);

            _appearance.SetData(item, SpringlockVisuals.Locked, true);
            _audio.PlayPredicted(springlock.LockSound, ent.Owner, ent.Owner);
        }
    }
}
