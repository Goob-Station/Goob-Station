using Content.Server._Moffstation.Power.EntitySystems;
using Content.Shared._Moffstation.BladeServer;

namespace Content.Server._Moffstation.BladeServer;

/// <summary>
/// This system extends <see cref="SharedBladeServerSystem"/> with sever-only power interactions.
/// </summary>
public sealed partial class BladeServerSystem : SharedBladeServerSystem
{
    [Dependency] private readonly InnerCableSystem _innerCable = default!;

    protected override void SetSlotPower(Entity<BladeServerRackComponent> entity, BladeSlot slot, bool powered)
    {
        base.SetSlotPower(entity, slot, powered);

        if (slot.Slot.ContainerSlot?.ID is not { } containerId)
            return;

        _innerCable.SetInnerProviderContainerConnectable(entity.Owner, containerId, powered);
    }
}
