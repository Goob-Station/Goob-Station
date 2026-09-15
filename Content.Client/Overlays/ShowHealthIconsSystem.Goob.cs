using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;

namespace Content.Client.Overlays;

public sealed partial class ShowHealthIconsSystem
{
    protected override void OnRefreshEquipmentHud(Entity<ShowHealthIconsComponent> ent,
        ref HeldRelayedEvent<RefreshEquipmentHudEvent<ShowHealthIconsComponent>> args)
    {
        if (ent.Comp.WorksInHands)
            base.OnRefreshEquipmentHud(ent, ref args);
    }
}