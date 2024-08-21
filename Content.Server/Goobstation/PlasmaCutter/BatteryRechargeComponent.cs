namespace Content.Server.Goobstation.Plasmacutter;

[RegisterComponent]
public sealed partial class BatteryRechargeComponent : Component
{
    /// <summary>
    /// Max material storage limit
    /// 3000 = 30 plasma sheets
    /// </summary>
    [DataField("storageMaxCapacity"), ViewVariables(VVAccess.ReadWrite)]
    public int StorageMaxCapacity = 3000;
}