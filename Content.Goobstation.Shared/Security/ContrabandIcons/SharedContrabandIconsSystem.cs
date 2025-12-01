using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Inventory;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.Security.ContrabandIcons;

/// <summary>
/// This is responsible for updating the contraband status icon
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    private bool _isEnabled = true;

    public override void Initialize()
    {
        base.Initialize();
        
        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);
    }
    protected void ContrabandDetect(EntityUid inventory, VisibleContrabandComponent component, SlotFlags slotFlags = SlotFlags.WITHOUT_POCKET)
    {
        if (!_isEnabled)
            return;
        var list = _detectorSystem.FindContraband(inventory, false, slotFlags);
        var isDetected = list.Count > 0;
        component.StatusIcon = StatusToIcon(isDetected ? ContrabandStatus.Contraband : ContrabandStatus.None);
        Dirty(inventory, component);
    }
    
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
