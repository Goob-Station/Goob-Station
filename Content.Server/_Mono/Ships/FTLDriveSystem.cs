using Content.Server.Power.Components;
using Content.Shared._Mono.Ships;
using Content.Shared._NF.Shuttles;
using Content.Shared.Power;

namespace Content.Server._Mono.Ships;

public sealed class FTLDriveSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<FTLDriveGeneratorComponent>> _drives = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FTLDriveGeneratorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FTLDriveGeneratorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FTLDriveGeneratorComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnStartup(EntityUid uid, FTLDriveGeneratorComponent generatorComponent, ComponentStartup args)
    {
        // Set initial power state
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiver))
            return;

        generatorComponent.Powered = powerReceiver.Powered;

        var grid = Transform(uid).GridUid;
        if (!TryComp(grid, out FTLDriveComponent? ftl))
            return;

        UpdateFtlDrives((grid.Value, ftl));
    }

    private void OnShutdown(EntityUid uid, FTLDriveGeneratorComponent generatorComponent, ComponentShutdown args)
    {
        var grid = Transform(uid).GridUid;
        if (!TryComp(grid, out FTLDriveComponent? ftl))
            return;

        UpdateFtlDrives((grid.Value, ftl));
    }

    private void OnPowerChanged(EntityUid uid, FTLDriveGeneratorComponent generatorComponent, ref PowerChangedEvent args)
    {
        generatorComponent.Powered = args.Powered;

        var grid = Transform(uid).GridUid;
        if (!TryComp(grid, out FTLDriveComponent? ftl))
            return;

        UpdateFtlDrives((grid.Value, ftl));
    }

    public void UpdateFtlDrives(Entity<FTLDriveComponent> shuttle)
    {
        _drives.Clear();
        _lookup.GetGridEntities(shuttle.Owner, _drives);
        FTLDriveGeneratorComponent? targetDrive = null;
        foreach (var (_, drive) in _drives)
        {
            if (!drive.Powered)
                continue;

            if (drive.Priority > (targetDrive?.Priority ?? -1))
                targetDrive = drive;
        }

        if (targetDrive == null)
            return;

        shuttle.Comp.Data = targetDrive.Data;
        Dirty(shuttle);
    }
}
