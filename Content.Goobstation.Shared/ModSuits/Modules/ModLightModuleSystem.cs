using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModLightModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedPointLightSystem _light = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModLightModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (GetLightTarget(ent, args.Suit) is not { } target)
            return;

        if (!_light.TryGetLight(target, out var light))
        {
            light = _light.EnsureLight(target);
            _light.SetRadius(target, 5f, light);
            _light.SetEnergy(target, 3f, light);
        }

        if (ent.Comp.Radius is { } radius)
            _light.SetRadius(target, radius, light);

        if (ent.Comp.Energy is { } energy)
            _light.SetEnergy(target, energy, light);

        if (ent.Comp.Color is { } color)
            _light.SetColor(target, color, light);

        _light.SetEnabled(target, true, light);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModLightModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (GetLightTarget(ent, args.Suit) is { } target && _light.TryGetLight(target, out var light))
            _light.SetEnabled(target, false, light);
    }

    private EntityUid? GetLightTarget(Entity<ModLightModuleComponent> ent, EntityUid suit)
    {
        return TryComp<ModSuitComponent>(suit, out var suitComp) ? _modSuit.GetPart((suit, suitComp), ent.Comp.Slot) : null;
    }
}
