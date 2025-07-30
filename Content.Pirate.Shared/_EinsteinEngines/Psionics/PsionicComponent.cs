using Content.Shared.DoAfter;
using Content.Shared.Psionics;
using Content.Shared.Random;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class PsionicComponent : Component
    {
        [DataField]
        public bool BypassManaCheck;

        /// <summary>
        ///     How close a Psion is to generating a new power. When Potentia reaches the NextPowerCost, it is "Spent" in order to "Buy" a random new power.
        ///     TODO: Psi-Potentiometry should be able to read how much Potentia a person has.
        /// </summary>
        [DataField]
        public float Potentia;

        /// <summary>
        ///     The base cost for new powers.
        /// </summary>
        [DataField]
        public float BaselinePowerCost = 100;

        /// <summary>
        ///     Each time a Psion rolls for a new power, they roll a number between 0 and 100, adding any relevant modifiers. This number is then added to Potentia,
        ///     meaning that it carries over between rolls. When a character has an amount of potentia equal to at least 100 * 2^(total powers), the potentia is then spent, and a power is generated.
        ///     This variable stores the cost of the next power.
        /// </summary>
        [DataField]
        public float NextPowerCost = 100;

        /// <summary>
        ///     The baseline chance of obtaining a psionic power when rolling for one.
        /// </summary>
        [DataField]
        public float Chance = 0.04f;

        /// <summary>
        ///     Whether or not a Psion has an available "Reroll" to spend on attempting to gain powers.
        /// </summary>
        [DataField]
        public bool CanReroll = true;

        /// <summary>
        ///     The Base amount of time (in minutes) this Psion is given the stutter effect if they become mindbroken.
        /// </summary>
        [DataField]
        public float MindbreakingStutterTime = 5;

        public string MindbreakingStutterCondition = "Stutter";

        public string MindbreakingStutterAccent = "StutteringAccent";

        /// <summary>
        ///     The message feedback given on mindbreak.
        /// </summary>
        [DataField]
        public string MindbreakingFeedback = "mindbreaking-feedback";

        /// <summary>
        /// </summary>
        [DataField]
        public string HardMindbreakingFeedback = "hard-mindbreaking-feedback";

        /// <summary>
        ///     How much should the odds of obtaining a Psionic Power be multiplied when rolling for one.
        /// </summary>
        [DataField]
        public float PowerRollMultiplier = 1f;

        /// <summary>
        ///     How much the odds of obtaining a Psionic Power should be multiplied when rolling for one.

        /// </summary>
        [DataField]
        public float PowerRollFlatBonus = 0;

        private (float, float) _baselineAmplification = (0.4f, 1.2f);

        /// <summary>
        ///     Use this datafield to change the range of Baseline Amplification.
        /// </summary>
        [DataField]
        private (float, float) _baselineAmplificationFactors = (0.4f, 1.2f);

        /// <summary>
        ///     All Psionics automatically possess a random amount of initial starting Amplification, regardless of if they have any powers or not.
        ///     The game will crash if Robust.Random is handed a (bigger number, smaller number), so the logic here prevents any funny business.
        /// </summary>
        public (float, float) BaselineAmplification
        {
            get { return _baselineAmplification; }
            private set
            {
                _baselineAmplification = (Math.Min(
                _baselineAmplificationFactors.Item1, _baselineAmplificationFactors.Item2),
                Math.Max(_baselineAmplificationFactors.Item1, _baselineAmplificationFactors.Item2));
            }
        }
        private (float, float) _baselineDampening = (0.4f, 1.2f);

        /// <summary>
        ///     Use this datafield to change the range of Baseline Amplification.
        /// </summary>
        [DataField]
        private (float, float) _baselineDampeningFactors = (0.4f, 1.2f);

        /// <summary>
        ///     All Psionics automatically possess a random amount of initial starting Dampening, regardless of if they have any powers or not.
        ///     The game will crash if Robust.Random is handed a (bigger number, smaller number), so the logic here prevents any funny business.
        /// </summary>
        public (float, float) BaselineDampening
        {
            get { return _baselineDampening; }
            private set
            {
                _baselineDampening = (Math.Min(
                _baselineDampeningFactors.Item1, _baselineDampeningFactors.Item2),
                Math.Max(_baselineDampeningFactors.Item1, _baselineDampeningFactors.Item2));
            }
        }

        /// <summary>
        ///     Whether this entity is capable of randomly rolling for powers.
        /// </summary>
        [DataField]
        public bool Roller = true;

        /// <summary>
        ///     Ifrits, revenants, etc are explicitly magical beings that shouldn't get mindbroken
        /// </summary>
        [DataField]
        public bool Removable = true;

        /// <summary>
        ///     The list of all powers currently active on a Psionic, by power Prototype.
        ///     TODO: Not in this PR due to scope, but this needs to go to Server and not Shared.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public HashSet<PsionicPowerPrototype> ActivePowers = new();

        /// <summary>
        ///     The list of each Psionic Power by prototype with entityUid.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public Dictionary<string, EntityUid?> Actions = new();

        /// <summary>
        ///     What sources of Amplification does this Psion have?
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly Dictionary<string, float> AmplificationSources = new();

        /// <summary>
        ///     A measure of how "Powerful" a Psion is.
        ///     TODO: Implement this in a separate PR.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float CurrentAmplification;

        /// <summary>
        ///     What sources of Dampening does this Psion have?
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly Dictionary<string, float> DampeningSources = new();

        /// <summary>
        ///     A measure of how "Controlled" a Psion is.
        ///     TODO: Implement this in a separate PR.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
        public float CurrentDampening;

        /// <summary>
        ///     How many "Slots" an entity has for psionic powers. This is not a hard limit, and is instead used for calculating the cost to generate new powers.
        ///     Exceeding this limit causes an entity to become a Glimmer Source.
        /// </summary>
        [DataField]
        public int PowerSlots = 1;

        /// <summary>
        ///     How many "Slots" are currently occupied by psionic powers.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int PowerSlotsTaken;

        /// <summary>
        ///     List of descriptors this entity will bring up for psychognomy. Used to remove
        ///     unneccesary subs for unique psionic entities like e.g. Oracle.
        /// </summary>
        [DataField]
        public List<string> PsychognomicDescriptors = new();

        /// Used for tracking what spell a Psion is actively casting
        [DataField]
        public DoAfterId? DoAfter;

        /// Popup to play if a Psion attempts to start casting a power while already casting one
        [DataField]
        public string AlreadyCasting = "already-casting";

        /// <summary>
        ///     The list of Familiars currently bound to this Psion.
        /// </summary>
        [DataField]
        public List<EntityUid> Familiars = new();

        /// <summary>
        ///     The maximum number of Familiars a Psion may bind.
        /// </summary>
        [DataField]
        public int FamiliarLimit = 1;

        /// <summary>
        ///     The list of all potential Assay messages that can be obtained from this Psion.
        /// </summary>
        [DataField]
        public List<string> AssayFeedback = new();

        /// <summary>
        ///     The list of powers that this Psion is eligible to roll new abilities from.
        ///     This generates the initial ability pool, but can also be modified by other systems.
        /// </summary>
        [DataField]
        public ProtoId<WeightedRandomPrototype> PowerPool = "RandomPsionicPowerPool";

        [DataField]
        public Dictionary<string, float> AvailablePowers = new();
    }
}
