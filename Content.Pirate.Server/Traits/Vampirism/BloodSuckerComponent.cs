namespace Content.Pirate.Server.Traits.Vampirism.Components

{
    [RegisterComponent]
    public sealed partial class BloodSuckerComponent : Component
    {
        /// <summary>
        /// How much to succ each time we succ.
        /// </summary>
        [DataField("unitsToSucc")]
        public float UnitsToSucc = 20f;

        /// <summary>
        /// The time (in seconds) that it takes to succ an entity.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan Delay = TimeSpan.FromSeconds(4);

        // ***INJECT WHEN SUCC***

        /// <summary>
        /// Whether to inject chems into a chemstream when we suck something.
        /// </summary>
        [DataField("injectWhenSucc")]
        public bool InjectWhenSucc = false;

        /// <summary>
        /// How many units of our injected chem to inject.
        /// </summary>
        [DataField("unitsToInject")]
        public float UnitsToInject = 5;

        /// <summary>
        /// Which reagent to inject.
        /// </summary>
        [DataField("injectReagent")]
        public string InjectReagent = "";

        /// <summary>
        /// Whether we need to web the thing up first...
        /// </summary>
        [DataField("webRequired")]
        public bool WebRequired = false;
    }
}
