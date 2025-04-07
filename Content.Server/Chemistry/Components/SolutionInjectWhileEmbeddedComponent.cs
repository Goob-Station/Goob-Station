using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


namespace Content.Server.Chemistry.Components;

/// <summary>
/// Used for embeddable entities that should try to inject a
/// contained solution into a target over time while they are embbeded into.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class SolutionInjectWhileEmbeddedComponent : BaseSolutionInjectOnEventComponent {
        ///<summary>
        ///The time at which the injection will happen.
        ///</summary>
        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
        public TimeSpan NextUpdate;
        
        ///<summary>
        ///The delay between each injection in seconds.
        ///</summary>
        [DataField]
        public TimeSpan UpdateInterval = TimeSpan.FromSeconds(3);

        // <Goobstation> Goobstation - Shot syringes injecting over time
        /// <summary>
        /// Maximum amount of injections this can do on one embed.
        /// Unlimited if null.
        /// </summary>
        [DataField]
        public int? MaxInjections;

        /// <summary>
        /// State: injections done on this entity so far.
        /// Set to null to uncap for this embed.
        /// Will be reset while not flying and not stuck in anything.
        /// </summary>
        [ViewVariables]
        public int? Injections = 0;
        // </Goobstation>
}

