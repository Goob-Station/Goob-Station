using Content.Goobstation.Shared.GPS.Components;
using Content.Shared.UserInterface;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.GPS;

public abstract class SharedGpsSystem : EntitySystem
{
    [Dependency] protected readonly SharedUserInterfaceSystem UiSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GPSComponent, BeforeActivatableUIOpenEvent>(OnOpen);

        SubscribeLocalEvent<GPSComponent, GpsSetTrackedEntityMessage>(OnSetTrackedEntity);
        SubscribeLocalEvent<GPSComponent, GpsSetGpsNameMessage>(OnSetGpsName);
        SubscribeLocalEvent<GPSComponent, GpsSetInDistressMessage>(OnSetInDistress);
        SubscribeLocalEvent<GPSComponent, GpsSetEnabledMessage>(OnSetEnabled);

        SubscribeLocalEvent<GPSComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(Entity<GPSComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateUi(ent);
    }

    private void OnOpen(Entity<GPSComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        Dirty(ent);
    }

    private void OnSetTrackedEntity(Entity<GPSComponent> ent, ref GpsSetTrackedEntityMessage args)
    {
        ent.Comp.TrackedEntity = args.NetEntity;
        DirtyField(ent.Owner, ent.Comp, nameof(GPSComponent.TrackedEntity));
        UpdateUi(ent);
    }

    private void OnSetGpsName(Entity<GPSComponent> ent, ref GpsSetGpsNameMessage args)
    {
        ent.Comp.GpsName = args.GpsName;
        DirtyField(ent.Owner, ent.Comp, nameof(GPSComponent.GpsName));
        UpdateUi(ent);
    }

    private void OnSetInDistress(Entity<GPSComponent> ent, ref GpsSetInDistressMessage args)
    {
        ent.Comp.InDistress = args.InDistress;
        DirtyField(ent.Owner, ent.Comp, nameof(GPSComponent.InDistress));
        UpdateUi(ent);
    }

    private void OnSetEnabled(Entity<GPSComponent> ent, ref GpsSetEnabledMessage args)
    {
        ent.Comp.Enabled = args.Enabled;
        DirtyField(ent.Owner, ent.Comp, nameof(GPSComponent.Enabled));
        UpdateUi(ent);
    }

    protected virtual void UpdateUi(Entity<GPSComponent> ent) { }
}
