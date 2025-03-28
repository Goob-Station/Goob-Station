using Content.Goobstation.Shared.Disease;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Disease;

public sealed partial class DiseaseProtectionSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseProtectionComponent, InventoryRelayedEvent<DiseaseIncomingSpreadAttemptEvent>>(OnIncomingSpread);
        SubscribeLocalEvent<DiseaseProtectionComponent, InventoryRelayedEvent<DiseaseOutgoingSpreadAttemptEvent>>(OnOutgoingSpread);
    }

    private void OnIncomingSpread(EntityUid uid, DiseaseProtectionComponent protection, ref InventoryRelayedEvent<DiseaseIncomingSpreadAttemptEvent> args)
    {
        args.Args.ApplyModifier(protection.Incoming);
    }

    private void OnOutgoingSpread(EntityUid uid, DiseaseProtectionComponent protection, ref InventoryRelayedEvent<DiseaseOutgoingSpreadAttemptEvent> args)
    {
        args.Args.ApplyModifier(protection.Outgoing);
    }
}
