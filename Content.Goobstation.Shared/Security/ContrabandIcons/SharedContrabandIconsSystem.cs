using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.GameTicking;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Content.Goobstation.Shared.Inventory;
using Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;

namespace Content.Shared._Goobstation.Security.ContrabandIcons;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    public void ContrabandDetect(EntityUid intentory, VisibleContrabandComponent component, SlotFlags slotFlags = SlotFlags.WITHOUT_POCKET)
    {
        var list = _detectorSystem.FindContraband(intentory, false, slotFlags);
        bool isDetected = list.Count > 0;
        component.StatusIcon = StatusToIcon(isDetected ? ContrabandStatus.Contraband : ContrabandStatus.None);
        Dirty(intentory, component);
    }
    private string StatusToIcon(ContrabandStatus status)
    {
        return status switch
        {
            ContrabandStatus.None => "NoneIcon",
            ContrabandStatus.Contraband => "ContrabandIconContraband",
            _ => "NoneIcon"
        };
    }
}
