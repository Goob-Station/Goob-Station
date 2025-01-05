using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Movement.Components
{
    /// <summary>
    /// Ignores gravity entirely.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class MovementIgnoreGravityComponent : Component
    {
        /// <summary>
        /// Whether or not gravity is on or off for this object.
        /// </summary>
        [DataField("gravityState")] public bool Weightless = false;
    }

    [NetSerializable, Serializable]
    public sealed class MovementIgnoreGravityComponentState : ComponentState
    {
        public bool Weightless;

        public MovementIgnoreGravityComponentState(MovementIgnoreGravityComponent component)
        {
            Weightless = component.Weightless;
        }
    }
}
