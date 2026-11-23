using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModStripComponentsModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModStripComponentsModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (!TryComp<ModSuitComponent>(args.Suit, out var suit))
            return;

        foreach (var part in suit.PartEntities.Values)
        {
            if (TerminatingOrDeleted(part))
                continue;

            foreach (var name in ent.Comp.Components)
            {
                if (Factory.TryGetRegistration(name, out var registration) && HasComp(part, registration.Type))
                    RemComp(part, registration.Type);
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModStripComponentsModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (!TryComp<ModSuitComponent>(args.Suit, out var suit))
            return;

        foreach (var part in suit.PartEntities.Values)
        {
            foreach (var name in ent.Comp.Components)
            {
                _modSuit.RestoreComponent(part, name);
            }
        }
    }
}
