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

        SubscribeLocalEvent<VisibleContrabandComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<VisibleContrabandComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<VisibleContrabandComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<VisibleContrabandComponent, GotEquippedHandEvent>(OnEquippedHands);
        SubscribeLocalEvent<VisibleContrabandComponent, GotUnequippedHandEvent>(OnUnequippedHands);
    }
    
    public void ContrabandDetect(EntityUid ent, VisibleContrabandComponent component)
    {
        var list = _detectorSystem.FindContraband(ent, false, SlotFlags.WITHOUT_POCKET);
        bool isDetected = list.Count > 0;
        component.StatusIcon = StatusToIcon(isDetected ? ContrabandStatus.None : ContrabandStatus.Contraband);
        Dirty(ent, component);
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

    private void OnMapInit(EntityUid uid, VisibleContrabandComponent component, MapInitEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnEquipped(EntityUid uid, VisibleContrabandComponent component, GotEquippedEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnUnequipped(EntityUid uid, VisibleContrabandComponent component, GotUnequippedEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnUnequippedHands(EntityUid uid, VisibleContrabandComponent component, GotUnequippedHandEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnEquippedHands(EntityUid uid, VisibleContrabandComponent component, GotEquippedHandEvent args)
    {
        ContrabandDetect(uid, component);
    }
}