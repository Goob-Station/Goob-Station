using Content.Shared.Nutrition.Components;

namespace Content.Shared.Nutrition.EntitySystems;

public sealed partial class ThirstSystem
{
    private void OnShutdown(EntityUid uid, ThirstComponent component, ComponentShutdown args)
    {
        _alerts.ClearAlertCategory(uid, component.ThirstyCategory);
        _movement.RefreshMovementSpeedModifiers(uid);
    }
}
