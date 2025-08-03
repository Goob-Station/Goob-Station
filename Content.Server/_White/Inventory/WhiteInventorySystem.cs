using Content.Shared._White.Inventory;

namespace Content.Server._White.Inventory;

public sealed partial class WhiteInventorySystem : SharedWhiteInventorySystem
{
    public override void Initialize()
    {
        base.Initialize();

        InitializeEquip();
    }
}
