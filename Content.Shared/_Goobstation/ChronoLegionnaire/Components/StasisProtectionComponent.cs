namespace Content.Shared._Goobstation.ChronoLegionnaire.Components
{
    /// <summary>
    /// Marks entity (clothing) that will give stasis immunity to wearer
    /// </summary>
    [RegisterComponent]
    public sealed partial class StasisProtectionComponent : Component
    {
        /// <summary>
        /// Stamina buff to entity wearer (until stun resist will be added)
        /// </summary>
        [DataField]
        public float StaminaModifier = 10f;
    }
}
