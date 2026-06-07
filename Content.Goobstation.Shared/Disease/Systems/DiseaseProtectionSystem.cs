using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Disease.Systems;

public sealed partial class DiseaseProtectionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseProtectionComponent, InventoryRelayedEvent<DiseaseIncomingSpreadAttemptEvent>>(OnIncomingSpread);
        SubscribeLocalEvent<DiseaseProtectionComponent, InventoryRelayedEvent<DiseaseOutgoingSpreadAttemptEvent>>(OnOutgoingSpread);
    }

    private void OnIncomingSpread(Entity<DiseaseProtectionComponent> ent, ref InventoryRelayedEvent<DiseaseIncomingSpreadAttemptEvent> args)
    {
        args.Args.ApplyModifier(ent.Comp.Incoming);
    }

    private void OnOutgoingSpread(Entity<DiseaseProtectionComponent> ent, ref InventoryRelayedEvent<DiseaseOutgoingSpreadAttemptEvent> args)
    {
        args.Args.ApplyModifier(ent.Comp.Outgoing);
    }
}
