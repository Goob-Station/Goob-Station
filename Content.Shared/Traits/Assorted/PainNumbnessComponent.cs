// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class PainNumbnessComponent : Component
{
    /// <summary>
    ///     The fluent string prefix to use when picking a random suffix
    ///     This is only active for those who have the pain numbness component
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> ForceSayNumbDataset = "ForceSayNumbDataset";
}
