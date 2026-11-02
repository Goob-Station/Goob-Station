using Content.Goobstation.Shared.ModSuits.Components;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModSuitSystem
{
    [SubscribeLocalEvent]
    private void OnUiActivate(Entity<ModSuitComponent> ent, ref ModSuitActivateMessage args)
    {
        if (ent.Comp.Wearer == args.Actor)
            TryToggleActivation(ent, args.Actor);
    }

    [SubscribeLocalEvent]
    private void OnUiTogglePart(Entity<ModSuitComponent> ent, ref ModSuitTogglePartMessage args)
    {
        if (ent.Comp.Wearer == args.Actor && ent.Comp.PartEntities.ContainsKey(args.Slot))
            TogglePart(ent, args.Slot, args.Actor);
    }

    [SubscribeLocalEvent]
    private void OnUiToggleAllParts(Entity<ModSuitComponent> ent, ref ModSuitToggleAllPartsMessage args)
    {
        if (ent.Comp.Wearer == args.Actor)
            ToggleAllParts(ent, args.Actor);
    }

    [SubscribeLocalEvent]
    private void OnUiModuleToggle(Entity<ModSuitComponent> ent, ref ModSuitModuleToggleMessage args)
    {
        if (TryGetInstalledModule(ent, args.Module, out var module))
            TryUseModule(module, args.Actor);
    }

    [SubscribeLocalEvent]
    private void OnUiModulePin(Entity<ModSuitComponent> ent, ref ModSuitModulePinMessage args)
    {
        if (TryGetInstalledModule(ent, args.Module, out var module) && TryComp<ModModuleComponent>(module, out var moduleComp))
            TogglePinned(ent, (module, moduleComp));
    }

    [SubscribeLocalEvent]
    private void OnUiModuleRemove(Entity<ModSuitComponent> ent, ref ModSuitModuleRemoveMessage args)
    {
        if (TryGetInstalledModule(ent, args.Module, out var module))
            TryRemoveModule(ent, module, args.Actor);
    }

    private bool TryGetInstalledModule(Entity<ModSuitComponent> ent, NetEntity netModule, out EntityUid module)
    {
        module = GetEntity(netModule);
        return ent.Comp.ModulesContainer.Contains(module);
    }
}
