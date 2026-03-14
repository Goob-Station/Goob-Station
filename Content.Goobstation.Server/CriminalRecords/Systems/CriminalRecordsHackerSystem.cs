using Content.Server.Power.EntitySystems;
using Content.Shared.CriminalRecords.Components;
using Content.Shared.CriminalRecords.Systems;

namespace Content.Goobstation.Server.CriminalRecords.Systems;

public sealed class CriminalRecordsHackerSystem : SharedCriminalRecordsHackerSystem
{
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CriminalRecordsHackerComponent, CriminalRecordsHackDoAfterEvent>(OnDoAfter, before: [typeof(Content.Server.CriminalRecords.Systems.CriminalRecordsHackerSystem)]);
    }

    private void OnDoAfter(Entity<CriminalRecordsHackerComponent> ent, ref CriminalRecordsHackDoAfterEvent args)
    {
        // Fall through if invalid, or target is powered.
        if (args.Cancelled || args.Handled || args.Target == null || _powerReceiverSystem.IsPowered(args.Target.Value))
        {
            return;
        }
        // Block Core if the target isn't powered.
        args.Handled = true;
    }
}
