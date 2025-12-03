using Content.Client.Overlays;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Security.ContrabandIcons;

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
