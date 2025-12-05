using Content.Goobstation.Shared.Security.ContrabandIcons;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;

namespace Content.Goobstation.Client.Security.Systems;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<VisibleContrabandComponent, ComponentStartup>(OnComponentStartup);
    }
    
    private void OnComponentStartup(EntityUid uid, VisibleContrabandComponent component, ComponentStartup args)
    {
        CheckAllContra(uid, component);
    }
}
