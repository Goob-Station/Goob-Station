using Content.Goobstation.Common.Standing;

namespace Content.Server._White.Traits.Assorted;

public sealed class LayingDownModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LayingDownModifierComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, LayingDownModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<LayingDownComponent>(uid, out var layingDown))
            return;

        layingDown.StandingUpTime *= component.LayingDownCooldownMultiplier;
        layingDown.SpeedModify *= component.DownedSpeedMultiplierMultiplier;
    }
}
