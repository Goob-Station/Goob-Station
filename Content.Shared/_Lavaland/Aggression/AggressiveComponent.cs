// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Aggression;

/// <summary>
///     Keeps track of whoever attacked our mob, so that it could prioritize or randomize targets.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AggressiveComponent : Component
{
    /// <summary>
    /// Active aggressors, that this aggressor will try to attack.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Aggressors = new();

    [ViewVariables]
    public TimeSpan NextUpdate;

    /// <summary>
    /// If specified, will forgive the target after it enters another map or
    /// goes farther than this range from it.
    /// </summary>
    [DataField]
    public float? ForgiveRange;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(10f);
}
