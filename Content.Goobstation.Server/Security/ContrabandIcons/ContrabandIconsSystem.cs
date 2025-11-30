using Content.Goobstation.Shared.Security.ContrabandIcons;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared._Goobstation.Security.ContrabandIcons;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.Security.ContrabandIcons;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VisibleContrabandComponent, MapInitEvent>(OnMapInit);
    }
    
    private void OnMapInit(EntityUid uid, VisibleContrabandComponent component, MapInitEvent args)
    {
        ContrabandDetect(uid, component, SlotFlags.WITHOUT_POCKET);
    }
}
