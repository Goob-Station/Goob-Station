using Content.Shared.CriminalRecords.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;

namespace Content.Goobstation.Shared.CriminalRecords.Systems;

public abstract class SharedCriminalRecordsHackerSystem : EntitySystem
{
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiverSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CriminalRecordsHackerComponent, BeforeInteractHandEvent>(OnBeforeInteractHand, before: [typeof(Content.Shared.CriminalRecords.Systems.SharedCriminalRecordsHackerSystem)]);
    }

    private void OnBeforeInteractHand(Entity<CriminalRecordsHackerComponent> ent, ref BeforeInteractHandEvent args)
    {
        // Block the event if target is not powered.
        if (args.Handled || _powerReceiverSystem.IsPowered(args.Target))
        {
            return;
        }

        args.Handled = true;
    }
}
