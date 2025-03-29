namespace Content.Goobstation.Shared.Inventory;

public sealed partial class GoobInventorySystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();
        InitializeRelays();
    }
}
