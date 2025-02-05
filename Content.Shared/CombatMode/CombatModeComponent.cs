using Content.Shared.MouseRotator;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.CombatMode
{
    /// <summary>
    ///     Stores whether an entity is in "combat mode"
    ///     This is used to differentiate between regular item interactions or
    ///     using *everything* as a weapon.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    [Access(typeof(SharedCombatModeSystem))]
    public sealed partial class CombatModeComponent : Component
    {
        #region Disarm

        /// <summary>
        /// Whether we are able to disarm. This requires our active hand to be free.
        /// False if it's toggled off for whatever reason, null if it's not possible.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("canDisarm")]
        public bool? CanDisarm;

        [DataField("disarmSuccessSound")]
        public SoundSpecifier DisarmSuccessSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

        [DataField("disarmFailChance")]
        public float BaseDisarmFailChance = 0.75f;

        #endregion

        [DataField("combatToggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string CombatToggleAction = "ActionCombatModeToggle";

        [DataField, AutoNetworkedField]
        public EntityUid? CombatToggleActionEntity;

        [ViewVariables(VVAccess.ReadWrite), DataField("isInCombatMode"), AutoNetworkedField]
        public bool IsInCombatMode;

        /// <summary>
        ///     Will add <see cref="MouseRotatorComponent"/> and <see cref="NoRotateOnMoveComponent"/>
        ///     to entities with this flag enabled that enter combat mode, and vice versa for removal.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool ToggleMouseRotator = true;

        // Reserve-NoScope-Start
        /// <summary>
        ///     If true, sets <see cref="MouseRotatorComponent.AngleTolerance"/> to 1 degree and <see cref="MouseRotatorComponent.Simple4DirMode"/>
        ///     to false when the owner enters combatmode. This is currently being tested as of 06.12.24,
        ///     so a simple bool switch should suffice.
        ///     Leaving AutoNetworking just in case shitmins need to disable it for someone. Will only take effect when re-enabling combat mode.
        /// </summary>
        /// <remarks>
        ///     No effect if <see cref="ToggleMouseRotator"/> is false.
        /// </remarks>
        [DataField, AutoNetworkedField]
        public bool SmoothRotation = true;
        // Reserve-NoScope-End
    }
}
