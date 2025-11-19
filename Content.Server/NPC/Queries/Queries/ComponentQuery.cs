// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Prototypes;

namespace Content.Server.NPC.Queries.Queries;

/// <summary>
/// Returns nearby components that match the specified components.
/// </summary>
public sealed partial class ComponentQuery : UtilityQuery
{
    [DataField("components", required: true)]
    public ComponentRegistry Components = default!;
}
