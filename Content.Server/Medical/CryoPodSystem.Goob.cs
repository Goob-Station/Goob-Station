using Content.Server.Temperature.Components;
using Content.Shared.Medical.Cryogenics;
using Robust.Shared.Containers;

namespace Content.Server.Medical;

public sealed partial class CryoPodSystem
{
    /// <summary>
    /// Used to handle inserting species with temperature transfer thresholds,
    /// by setting their thresholds to the default
    /// </summary>
    private void OnInserted(EntityUid uid, CryoPodComponent cryoComp, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != cryoComp.BodyContainer.ID)
            return;

        if (!TryComp<TemperatureComponent>(args.Entity, out var tempComp))
            return;

        // Ensure we have the component, it should be already on the Entity, but just in-case.
        EnsureComp<InsideCryoPodComponent>(args.Entity, out var insideComp);

        // Store the original threshold or mark it as null if it's already the default
        if (!MathHelper.CloseTo(tempComp.AtmosTemperatureTransferEfficiency, 0.1f, 0.01f))
        {
            insideComp.OriginalAtmosTemperatureTransferEfficiency = tempComp.AtmosTemperatureTransferEfficiency;
            tempComp.AtmosTemperatureTransferEfficiency = 0.1f;
        }
        else
            insideComp.OriginalAtmosTemperatureTransferEfficiency = null;
    }

    /// <summary>
    /// Used to handle ejecting species with temperature transfer thresholds,
    /// this resets their thresholds to their stored values.
    /// </summary>
    private void OnRemoved(EntityUid uid, CryoPodComponent cryoComp, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != cryoComp.BodyContainer.ID)
            return;

        if (!TryComp<InsideCryoPodComponent>(args.Entity, out var insideCom)
            || !TryComp<TemperatureComponent>(args.Entity, out var tempComp))
            return;

        // Reset the threshold to the original value if it was stored
        if (insideCom.OriginalAtmosTemperatureTransferEfficiency != null)
            tempComp.AtmosTemperatureTransferEfficiency = insideCom.OriginalAtmosTemperatureTransferEfficiency.Value;
    }
}
