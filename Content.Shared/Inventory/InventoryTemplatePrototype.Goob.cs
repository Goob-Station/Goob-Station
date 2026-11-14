namespace Content.Shared.Inventory;

public sealed partial class SlotDefinition
{
    [DataField] public bool StrictWhitelist { get; private set; }
}
