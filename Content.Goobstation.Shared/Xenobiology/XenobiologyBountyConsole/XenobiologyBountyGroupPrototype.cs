using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;

/// <summary>
/// Used to categorize bounties for different purposes
/// </summary>
[Prototype]
public sealed partial class XenobiologyBountyGroupPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;
}
