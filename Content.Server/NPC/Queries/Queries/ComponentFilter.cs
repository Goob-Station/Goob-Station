// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Server.NPC.Queries.Queries;

public sealed partial class ComponentFilter : UtilityQueryFilter
{
    /// <summary>
    /// Components to filter for.
    /// </summary>
    [DataField("components", required: true)]
    public ComponentRegistry Components = new();

    // Goobstation
    [DataField]
    public bool Invert;
}
