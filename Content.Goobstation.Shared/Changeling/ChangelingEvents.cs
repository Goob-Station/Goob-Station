using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Changeling;

/// <summary>
/// Called when a changeling generates chemicals.
/// </summary>
/// <param name="Modifier"> The modifier that will be applied to a changeling's chemical generation. </param>
[ByRefEvent]
public record struct ChangelingChemicalRegenEvent(float BaseModifier) : IInventoryRelayEvent
{
    public float Modifier = BaseModifier;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

#region Evolution events

[DataDefinition]
public sealed partial class AugmentedEyesightPurchasedEvent : EntityEventArgs;

[DataDefinition]
public sealed partial class AwakenedInstinctPurchasedEvent : EntityEventArgs;

[DataDefinition]
public sealed partial class VoidAdaptionPurchasedEvent : EntityEventArgs;

#endregion
