using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

public sealed partial class JobPrototype
{
    /// <summary>
    ///     MRP visibility flag.
    ///     null: neutral (show regardless of CVar)
    ///     true: show only when MRP is enabled
    ///     false: show only when MRP is disabled
    /// </summary>
    [DataField("mrp")]
    public bool? Mrp { get; private set; } = null;
}
