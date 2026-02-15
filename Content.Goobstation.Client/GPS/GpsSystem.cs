using Content.Goobstation.Shared.GPS;
using Content.Goobstation.Shared.GPS.Components;

namespace Content.Goobstation.Client.GPS;

public sealed class GpsSystem : SharedGpsSystem
{
    protected override void UpdateUi(Entity<GPSComponent> ent)
    {
        if (UiSystem.TryGetOpenUi<GpsBoundUserInterface>(ent.Owner,
                GpsUiKey.Key,
                out var bui))
            bui.UpdateWindow();
    }
}
