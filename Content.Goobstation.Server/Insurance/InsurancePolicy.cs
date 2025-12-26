using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Insurance;

[DataDefinition]
public sealed partial class InsurancePolicy
{
    /// <summary>
    /// The entities this policy can be applied to.
    /// </summary>
    [DataField]
    public EntityWhitelist ValidEntities;

    /// <summary>
    /// Extra entities to be spawned with the compensation package.
    /// </summary>
    [DataField]
    public List<EntProtoId>? ExtraCompensationItems;

    /// <summary>
    /// Whether to automatically include the prototype of the entity the policy is applied to.
    /// </summary>
    [DataField]
    public bool IncludeTarget = true;

    /// <summary>
    /// The delay between the insured entity being destroyed and the drop pod being spawned.
    /// No delay if null.
    /// </summary>
    [DataField]
    public float? DropDelay = null;

    /// <summary>
    /// The radius to search for valid positions to drop compensation packages.
    /// </summary>
    [DataField]
    public int DropRadius = 2;
}
