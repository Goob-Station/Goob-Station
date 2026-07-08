// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.Laws.Components;

/// <summary>
/// Whenever an entity is inserted with silicon laws it will update the relevant entity's laws.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SiliconLawUpdaterComponent : Component
{
    /// <summary>
    /// Entities to update
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components;

    /// <summary>
    /// Goob edit: the last lawset that was loaded with this updater.
    /// </summary>
    [ViewVariables]
    public ProtoId<SiliconLawsetPrototype> LastLawset = "Crewsimov";
}
