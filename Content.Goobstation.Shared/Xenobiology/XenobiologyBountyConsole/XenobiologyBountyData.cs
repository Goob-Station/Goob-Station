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

    public XenobiologyBountyData(XenobiologyBountyPrototype bounty, int uniqueIdentifier)
    {
        Bounty = bounty.ID;
        Id = $"{bounty.IdPrefix}{uniqueIdentifier:D3}";
    }
}
