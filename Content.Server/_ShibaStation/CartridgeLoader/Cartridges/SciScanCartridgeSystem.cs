using Content.Server.Anomaly.Components;
using Content.Shared.CartridgeLoader;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class SciScanCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SciScanCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<SciScanCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
    }

    private void OnCartridgeAdded(Entity<SciScanCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        var anomalyScanner = EnsureComp<AnomalyScannerComponent>(args.Loader);
    }

    private void OnCartridgeRemoved(Entity<SciScanCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        if (!_cartridgeLoaderSystem.HasProgram<SciScanCartridgeComponent>(args.Loader))
        {
            RemComp<AnomalyScannerComponent>(args.Loader);
        }
    }
}
