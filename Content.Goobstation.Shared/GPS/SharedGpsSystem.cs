using Content.Goobstation.Shared.GPS.Components;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.GPS;

public abstract class SharedGpsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GPSComponent, ComponentInit>(OnGpsInit);
    }

    private void OnGpsInit(EntityUid uid, GPSComponent component, ComponentInit args)
    {
        if (string.IsNullOrWhiteSpace(component.GpsName))
            component.GpsName = "GPS-" + uid;
    }
}
