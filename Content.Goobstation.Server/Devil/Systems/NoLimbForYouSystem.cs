using Content.Goobstation.Shared.Devil.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Part;

namespace Content.Goobstation.Server.Devil.Systems;

/// <summary>
/// Removes any limb reattached to a slot the entity signed away to the Devil.
/// </summary>
public sealed class NoLimbForYouSystem : EntitySystem
{
    [Dependency] private readonly WoundSystem _wounds = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoLimbForYouComponent, BodyPartAddedEvent>(OnBodyPartAdded);
    }

    private void OnBodyPartAdded(EntityUid uid, NoLimbForYouComponent comp, ref BodyPartAddedEvent args)
    {
        if (!comp.ForbiddenSlots.Contains(args.Slot))
            return;

        if (!TryComp<WoundableComponent>(args.Part, out var woundable) || !woundable.ParentWoundable.HasValue)
            return;

        _wounds.AmputateWoundableSafely(woundable.ParentWoundable.Value, args.Part, woundable);
        QueueDel(args.Part);
    }
}
