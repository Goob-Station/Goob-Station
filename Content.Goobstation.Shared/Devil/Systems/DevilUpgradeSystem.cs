using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;

namespace Content.Goobstation.Server.Devil.Grip;

public sealed class DevilGripUpgradeSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<DevilComponent, DevilGripCurseRotPurchasedEvent>(OnCurseRot);
        SubscribeLocalEvent<DevilComponent, DevilGripEnhancedPurchasedEvent>(OnEnhanced);
    }

    private void OnCurseRot(EntityUid uid, DevilComponent comp, ref DevilGripCurseRotPurchasedEvent ev)
    {
        EnsureComp<GripSidegradeRotComponent>(uid);
    }

    private void OnEnhanced(EntityUid uid, DevilComponent comp, ref DevilGripEnhancedPurchasedEvent ev)
    {
        EnsureComp<GripSidegradeStunComponent>(uid);
    }
}
