// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._White.Xenomorphs.Queen;

/// <summary>
/// Marks a xenomorph whose Queen Finder alert uses hive leadership priority
/// (Empress → Queen → egg-sack / ovipositor → Praetorian) and can cycle on click.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class QueenFinderComponent : Component
{
    /// <summary>
    /// Currently tracked hive leader. Null means pick highest priority on next refresh.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? SelectedTarget;
}
