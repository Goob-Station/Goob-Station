// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Hood.Phone;

/// <summary>
/// Provides a phone's server-assigned, round-local network identity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SimCardComponent : Component
{
    /// <summary>
    /// The fictional number assigned by <c>SimCardSystem</c> on the server.
    /// A null value means assignment has not completed or the number pool is exhausted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public uint? Number;
}
