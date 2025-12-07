using System.Linq;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;
using Content.Shared.Inventory;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Security.ContrabandIcons;

public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private readonly ProtoId<ContrabandIconPrototype> _contrabandIconProto = "ContrabandIconContraband";
    private readonly ProtoId<ContrabandIconPrototype> _noneIconProto = "ContrabandIconNone";

    private EntityQuery<VisibleContrabandComponent> _visibleContrabandQuery;

    public override void Initialize()
    {
        base.Initialize();
        _visibleContrabandQuery = GetEntityQuery<VisibleContrabandComponent>();
        _prototypeManager.Index(_contrabandIconProto);
        _prototypeManager.Index(_noneIconProto);
    }

    private ProtoId<ContrabandIconPrototype> StatusToIcon(ContrabandStatus status)
    {
        return status == ContrabandStatus.Contraband ? _contrabandIconProto : _noneIconProto;
    }

    protected void CheckAllContra(EntityUid uid)
    {
        uid = GetHighestContainerOwner(uid);

        if (!_visibleContrabandQuery.TryComp(uid, out var visible))
            return;

        var contraband = _detectorSystem.FindContraband(uid, false, SlotFlags.WITHOUT_POCKET);
        if (contraband == null) // this is stupid but it works
            return;

        visible.VisibleItems = contraband.ToHashSet();
        var status = visible.VisibleItems.Count > 0 ? ContrabandStatus.Contraband : ContrabandStatus.None;
        UpdateStatusIcon(visible, uid, status);
    }

    protected void UpdateStatusIcon(VisibleContrabandComponent comp, EntityUid uid, ContrabandStatus status)
    {
        var newStatus = StatusToIcon(status);
        if (comp.StatusIcon == newStatus)
            return;

        comp.StatusIcon = newStatus;
        if (_net.IsServer)
            Dirty(uid, comp);
    }

    private EntityUid GetHighestContainerOwner(EntityUid uid)
    {
        while (_inventory.TryGetContainingEntity(uid, out var containerEntity))
        {
            uid = containerEntity.Value;
        }

        return uid;
    }
}