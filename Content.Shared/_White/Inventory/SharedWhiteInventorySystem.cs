namespace Content.Shared._White.Inventory;

public abstract partial class SharedWhiteInventorySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        InitializeEquip();
    }
}
