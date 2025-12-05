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
        // Force an initial update of the icon on startup
        var newStatus = component.VisibleItems.Count > 0 ? StatusToIcon(ContrabandStatus.Contraband) : StatusToIcon(ContrabandStatus.None);
        if (component.StatusIcon != newStatus)
        {
            component.StatusIcon = newStatus;
            Dirty(uid, component);
        }
    }

    /// <summary>
    ///     returns the icon string based on status enum
    /// </summary>
    private string StatusToIcon(ContrabandStatus status)
    {
        return status switch
        {
            ContrabandStatus.None => "ContrabandIconNone",
            ContrabandStatus.Contraband => "ContrabandIconContraband",
            _ => "ContrabandIconNone"
        };
    }
}
