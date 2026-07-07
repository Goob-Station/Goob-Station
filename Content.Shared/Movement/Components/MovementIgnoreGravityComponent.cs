// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Movement.Components
{
    /// <summary>
    /// Ignores gravity entirely.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class MovementIgnoreGravityComponent : Component
    {
        /// <summary>
        /// Whether gravity is on or off for this object. This will always override the current Gravity State.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool Weightless;
    }
}
