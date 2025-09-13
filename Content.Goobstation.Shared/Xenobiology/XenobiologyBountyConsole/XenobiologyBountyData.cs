using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;

[DataDefinition, NetSerializable, Serializable]
public sealed partial class XenobiologyBountyData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField]
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// The prototype containing information about the bounty.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<XenobiologyBountyPrototype> Bounty { get; private set; } = string.Empty;

    /// <summary>
    /// The current point multiplier for the bounty.
    /// This fluctuates every tme you sell.
    /// </summary>
    [DataField]
    public float MinMultiplier = 0.7f;

    [DataField]
    public float MaxMultiplier = 2f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentMultiplier = 1f;

    /// <summary>
    /// Every reset, it will move X% of the way back to the initial mult.
    /// 0.3 is 30%, etcetera.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float LerpBlend = 0.3f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float InitialMultiplier;

    public XenobiologyBountyData(XenobiologyBountyPrototype bounty, int uniqueIdentifier)
    {
        Bounty = bounty.ID;
        Id = $"{bounty.IdPrefix}{uniqueIdentifier:D3}";
    }
}
