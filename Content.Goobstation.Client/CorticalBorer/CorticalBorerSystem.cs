using Content.Goobstation.Shared.CorticalBorer;
using Content.Goobstation.Shared.CorticalBorer.Components;
using Content.Shared.Alert.Components;

namespace Content.Goobstation.Client.CorticalBorer;

/// <inheritdoc/>
public sealed class CorticalBorerSystem : SharedCorticalBorerSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CorticalBorerComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
    }

    private void OnGetCounterAmount(Entity<CorticalBorerComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.ChemicalAlert != args.Alert)
            return;

        args.Amount = ent.Comp.ChemicalPoints;
    }
}
