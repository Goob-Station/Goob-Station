using Content.Shared.Movement.Systems;

namespace Content.Shared._Afterlight.Silicons.Synths.Battery;

public sealed partial class SynthBatteryEffectsSystem : EntitySystem
{
    [Dependency] private SynthBatteryAlertSystem _alerts = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SynthBatteryComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SynthBatteryComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SynthBatteryComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<SynthBatteryComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    public void SetUnpowered(Entity<SynthBatteryComponent> ent, bool unpowered)
    {
        if (ent.Comp.Unpowered == unpowered)
            return;

        ent.Comp.Unpowered = unpowered;
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnStartup(Entity<SynthBatteryComponent> ent, ref ComponentStartup args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnShutdown(Entity<SynthBatteryComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearBatteryAlerts(ent);
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnHandleState(Entity<SynthBatteryComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRefreshMovementSpeed(Entity<SynthBatteryComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Unpowered)
            args.ModifySpeed(ent.Comp.UnpoweredWalkSpeedModifier, ent.Comp.UnpoweredSprintSpeedModifier);
    }
}
