using Content.Shared.Damage;

namespace Content.Server.Psionics
{
    /// <summary>
    ///     A component for weapons intended to have special effects when wielded against Psionic Entities.
    /// </summary>
    [RegisterComponent]
    public sealed partial class AntiPsionicWeaponComponent : Component
    {

        [DataField(required: true)]
        public DamageModifierSet Modifiers = default!;

        [DataField]
        public float PsychicStaminaDamage = 30f;

        /// <summary>
        ///     How long (in seconds) should this weapon temporarily disable powers
        /// </summary>
        [DataField]
        public float DisableDuration = 10f;

        /// <summary>
        ///     The chances of this weapon temporarily disabling psionic powers
        /// </summary>
        [DataField]
        public float DisableChance = 0.3f;

        /// <summary>
        ///     The condition to be inflicted on a Psionic entity
        /// </summary>
        [DataField]
        public string DisableStatus = "PsionicsDisabled";

        /// <summary>
        ///     Whether or not the user of this weapon risks Punishment by the gods if they dare use it on non-Psionic Entities
        /// </summary
        [DataField]
        public bool Punish = true;

        /// <summary>
        ///     The odds of divine punishment per non-Psionic Entity attacked
        /// </summary>
        [DataField]
        public float PunishChances = 0.5f;

        /// <summary>
        ///     How much Shock damage to take when Punish(ed) by the gods for using this weapon
        /// </summary>
        [DataField]
        public int PunishSelfDamage = 20;

        /// <summary>
        ///     How long (in seconds) should the user be stunned when punished by the gods
        /// </summary>
        [DataField]
        public float PunishStunDuration = 5f;
    }
}
