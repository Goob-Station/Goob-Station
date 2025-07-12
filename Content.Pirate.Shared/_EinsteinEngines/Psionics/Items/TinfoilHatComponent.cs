namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class TinfoilHatComponent : Component
    {
        public bool IsActive = false;

        [DataField("passthrough")]
        public bool Passthrough = false;

        /// <summary>
        /// Whether this will turn to ash when its psionically fried.
        /// </summary>
        [DataField("destroyOnFry")]
        public bool DestroyOnFry = true;
    }
}
