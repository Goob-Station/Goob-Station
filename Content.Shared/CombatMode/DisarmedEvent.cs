namespace Content.Shared.CombatMode
{
    public sealed class DisarmedEvent : HandledEntityEventArgs
    {
        /// <summary>
        ///     The entity being disarmed.
        /// </summary>
        public EntityUid Target { get; init; }

        /// <summary>
        ///     The entity performing the disarm.
        /// </summary>
        public EntityUid Source { get; init; }

        /// <summary>
        ///     Probability to disarm in addition to shoving.
        /// </summary>
        public float DisarmProbability { get; init; }

        /// <summary>
        ///     Prefix for the popup message that will be displayed on a successful push.
        ///     Should be set before returning.
        /// </summary>
        public string PopupPrefix { get; set; } = "";

        /// <summary>
        ///     Whether the entity was successfully stunned from a shove.
        /// </summary>
        public bool IsStunned { get; set; }

        /// <summary>
        ///     Potential stamina damage if this disarm results in a shove.
        /// </summary>
        public float StaminaDamage { get; init; }

        /// <summary>
        ///     Whether the entity was successfully stunned from a shove.
        /// </summary>
        public bool WasDisarmed { get; set; }

    }
}
