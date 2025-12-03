using Content.Client.Overlays;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using ShowContrabandIconsComponent = Content.Goobstation.Shared.Security.ContrabandIcons.Components.ShowContrabandIconsComponent;
using VisibleContrabandComponent = Content.Goobstation.Shared.Security.ContrabandIcons.Components.VisibleContrabandComponent;

namespace Content.Goobstation.Client.Security.Systems;

public sealed class ShowContrabandIconsSystem : EquipmentHudSystem<ShowContrabandIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VisibleContrabandComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, VisibleContrabandComponent component, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;
        
        if (_prototype.TryIndex(component.StatusIcon, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype);
    }
}
