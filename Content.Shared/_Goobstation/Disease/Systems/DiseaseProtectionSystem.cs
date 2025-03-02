using Content.Shared.Disease;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameObjects;

namespace Content.Shared.Disease;

public sealed partial class DiseaseProtectionSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseProtectionComponent, DiseaseIncomingSpreadAttemptEvent>(OnIncomingSpread);
        SubscribeLocalEvent<DiseaseProtectionComponent, DiseaseOutgoingSpreadAttemptEvent>(OnOutgoingSpread);
    }

    private void OnIncomingSpread(EntityUid uid, DiseaseProtectionComponent protection, ref DiseaseIncomingSpreadAttemptEvent args)
    {
        if (protection.Incoming.ContainsKey(args.Type))
            args.Power += protection.Incoming[args.Type];
    }

    private void OnOutgoingSpread(EntityUid uid, DiseaseProtectionComponent protection, ref DiseaseOutgoingSpreadAttemptEvent args)
    {
        if (protection.Outgoing.ContainsKey(args.Type))
            args.Power += protection.Outgoing[args.Type];
    }
}
