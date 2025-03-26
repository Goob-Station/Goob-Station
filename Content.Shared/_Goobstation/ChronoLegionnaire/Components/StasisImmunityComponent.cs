namespace Content.Shared._Goobstation.ChronoLegionnaire.Components
{
    /// <summary>
    /// Marks an entity that cannot be affect by stasis
    /// </summary>
    [RegisterComponent]
    public sealed partial class StasisImmunityComponent : Component
    {
        /// <summary>
        /// Will the stasis immunity go away with stasis protection?
        /// </summary>
        [DataField]
        public bool DependsOnProtection = true;
    }
}
