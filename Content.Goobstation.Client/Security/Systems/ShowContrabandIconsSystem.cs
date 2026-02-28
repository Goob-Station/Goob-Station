using Content.Client.Overlays;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Goobstation.Common.CCVar;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.Security.Systems;

public sealed class ShowContrabandIconsSystem : EquipmentHudSystem<ShowContrabandIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    private bool _isEnabled = true;
    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);
        SubscribeLocalEvent<VisibleContrabandComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, VisibleContrabandComponent component, ref GetStatusIconsEvent ev)
    {
        if (!IsActive || !_isEnabled)
            return;
        
        if (_prototype.TryIndex(component.StatusIcon, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype);
    }
}
