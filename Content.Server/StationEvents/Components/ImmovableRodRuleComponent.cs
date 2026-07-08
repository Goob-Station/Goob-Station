// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.StationEvents.Events;
using Content.Shared.Storage;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(ImmovableRodRule))]
public sealed partial class ImmovableRodRuleComponent : Component
{
    ///     List of possible rods and spawn probabilities.
    /// </summary>
    [DataField]
    public List<EntitySpawnEntry> RodPrototypes = new();
}