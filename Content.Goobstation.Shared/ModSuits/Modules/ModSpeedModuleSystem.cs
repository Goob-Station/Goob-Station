using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Movement.Systems;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModSpeedModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModSpeedModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        Refresh(args.Suit);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModSpeedModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        Refresh(args.Suit);
    }

    [SubscribeLocalEvent]
    private void OnRefreshSpeed(Entity<ModSpeedModuleComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.WalkModifier, ent.Comp.SprintModifier);
    }

    private void Refresh(EntityUid suit)
    {
        if (TryComp<ModSuitComponent>(suit, out var suitComp))
            _modSuit.RefreshWearerSpeed((suit, suitComp));
    }
}
