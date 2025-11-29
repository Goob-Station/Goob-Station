using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.GameTicking;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Content.Goobstation.Shared.Inventory;

namespace Content.Shared._Goobstation.Security.ContrabandIcons;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<VisibleContrabandComponent, GotEquippedEvent>(OnInsertedIntoInventory);
    }

    private void OnInsertedIntoInventory(EntityUid uid, VisibleContrabandComponent component,
        GotEquippedEvent args)
    {
        
    }

    public void ContrabandDetect(EntityUid ent, VisibleContrabandComponent component)
    {
        bool IsDetected = false;
        var list = _detectorSystem.FindContraband(ent, false);
        IsDetected = list.Count > 0;
        component.StatusIcon = StatusToIcon(IsDetected ? ContrabandStatus.None : ContrabandStatus.Contraband);
    }

    private string StatusToIcon(ContrabandStatus status)
    {
        return status switch
        {
            ContrabandStatus.None => "NoneIcon",
            ContrabandStatus.Contraband => "ContrabandIcon",
            _ => "NoneIcon"
        };
    }
    
}