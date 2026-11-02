using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Nutrition.Components;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModMouthholeModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModMouthholeModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (GetPart(ent, args.Suit) is { } part)
            RemComp<IngestionBlockerComponent>(part);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModMouthholeModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (GetPart(ent, args.Suit) is { } part)
            _modSuit.RestoreComponent(part, Factory.GetComponentName<IngestionBlockerComponent>());
    }

    private EntityUid? GetPart(Entity<ModMouthholeModuleComponent> ent, EntityUid suit)
    {
        return TryComp<ModSuitComponent>(suit, out var suitComp) ? _modSuit.GetPart((suit, suitComp), ent.Comp.Slot) : null;
    }
}
