// SPDX-FileCopyrightText: 2020 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 GlassEclipse <tsymall5@gmail.com>
// SPDX-FileCopyrightText: 2020 nuke <47336974+nuke-makes-games@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ilushkins33 <128389588+Ilushkins33@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Body.Components
{
    [RegisterComponent, Access(typeof(BloodstreamSystem), typeof(ReactionMixerSystem))]
    public sealed partial class BloodstreamComponent : Component
    {
        public static string DefaultChemicalsSolutionName = "chemicals";
        public static string DefaultBloodSolutionName = "bloodstream";
        public static string DefaultBloodTemporarySolutionName = "bloodstreamTemporary";

        /// <summary>
        /// The next time that blood level will be updated and bloodloss damage dealt.
        /// </summary>
        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan NextUpdate;

        /// <summary>
        /// The interval at which this component updates.
        /// </summary>
        [DataField]
        public TimeSpan UpdateInterval = TimeSpan.FromSeconds(3);

        /// <summary>
        ///     How much is this entity currently bleeding?
        ///     Higher numbers mean more blood lost every tick.
        ///
        ///     Goes down slowly over time, and items like bandages
        ///     or clotting reagents can lower bleeding.
        /// </summary>
        /// <remarks>
        ///     This generally corresponds to an amount of damage and can't go above 100.
        /// </remarks>
        [ViewVariables(VVAccess.ReadWrite)]
        public float BleedAmount;

        /// <summary>
        ///     How much should bleeding be reduced every update interval?
        /// </summary>
        [DataField]
        public float BleedReductionAmount = 0.33f;

        /// <summary>
        ///     How high can <see cref="BleedAmount"/> go?
        /// </summary>
        [DataField]
        public float MaxBleedAmount = 10.0f;

        /// <summary>
        ///     What percentage of current blood is necessary to avoid dealing blood loss damage?
        /// </summary>
        [DataField]
        public float BloodlossThreshold = 0.9f;

        /// <summary>
        ///     The base bloodloss damage to be incurred if below <see cref="BloodlossThreshold"/>
        ///     The default values are defined per mob/species in YML.
        /// </summary>
        [DataField(required: true)]
        public DamageSpecifier BloodlossDamage = new();

        /// <summary>
        ///     The base bloodloss damage to be healed if above <see cref="BloodlossThreshold"/>
        ///     The default values are defined per mob/species in YML.
        /// </summary>
        [DataField(required: true)]
        public DamageSpecifier BloodlossHealDamage = new();

        // TODO shouldn't be hardcoded, should just use some organ simulation like bone marrow or smth.
        /// <summary>
        ///     How much reagent of blood should be restored each update interval?
        /// </summary>
        [DataField]
        public FixedPoint2 BloodRefreshAmount = 1.0f;

        /// <summary>
        ///     How much blood needs to be in the temporary solution in order to create a puddle?
        /// </summary>
        [DataField]
        public FixedPoint2 BleedPuddleThreshold = 1.0f;

        /// <summary>
        ///     A modifier set prototype ID corresponding to how damage should be modified
        ///     before taking it into account for bloodloss.
        /// </summary>
        /// <remarks>
        ///     For example, piercing damage is increased while poison damage is nullified entirely.
        /// </remarks>
        [DataField]
        public ProtoId<DamageModifierSetPrototype> DamageBleedModifiers = "BloodlossHuman";

        /// <summary>
        ///     The sound to be played when a weapon instantly deals blood loss damage.
        /// </summary>
        [DataField]
        public SoundSpecifier InstantBloodSound = new SoundCollectionSpecifier("blood");

        /// <summary>
        ///     The sound to be played when some damage actually heals bleeding rather than starting it.
        /// </summary>
        [DataField]
        public SoundSpecifier BloodHealedSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

        /// <summary>
        /// The minimum amount damage reduction needed to play the healing sound/popup.
        /// This prevents tiny amounts of heat damage from spamming the sound, e.g. spacing.
        /// </summary>
        [DataField]
        public float BloodHealedSoundThreshold = -0.1f;

        // TODO probably damage bleed thresholds.

        /// <summary>
        ///     Max volume of internal chemical solution storage
        /// </summary>
        [DataField]
        public FixedPoint2 ChemicalMaxVolume = FixedPoint2.New(250);

        /// <summary>
        ///     Max volume of internal blood storage,
        ///     and starting level of blood.
        /// </summary>
        [DataField]
        public FixedPoint2 BloodMaxVolume = FixedPoint2.New(300);

        /// <summary>
        ///     Which reagent is considered this entities 'blood'?
        /// </summary>
        /// <remarks>
        ///     Slime-people might use slime as their blood or something like that.
        /// </remarks>
        [DataField]
        public ProtoId<ReagentPrototype> BloodReagent = "Blood";

        /// <summary>Name/Key that <see cref="BloodSolution"/> is indexed by.</summary>
        [DataField]
        public string BloodSolutionName = DefaultBloodSolutionName;

        /// <summary>Name/Key that <see cref="ChemicalSolution"/> is indexed by.</summary>
        [DataField]
        public string ChemicalSolutionName = DefaultChemicalsSolutionName;

        /// <summary>Name/Key that <see cref="TemporarySolution"/> is indexed by.</summary>
        [DataField]
        public string BloodTemporarySolutionName = DefaultBloodTemporarySolutionName;

        /// <summary>
        ///     Internal solution for blood storage
        /// </summary>
        [ViewVariables]
        public Entity<SolutionComponent>? BloodSolution;

        /// <summary>
        ///     Internal solution for reagent storage
        /// </summary>
        [ViewVariables]
        public Entity<SolutionComponent>? ChemicalSolution;

        /// <summary>
        ///     Temporary blood solution.
        ///     When blood is lost, it goes to this solution, and when this
        ///     solution hits a certain cap, the blood is actually spilled as a puddle.
        /// </summary>
        [ViewVariables]
        public Entity<SolutionComponent>? TemporarySolution;

        /// <summary>
        /// Variable that stores the amount of status time added by having a low blood level.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan StatusTime;

        [DataField]
        public ProtoId<AlertPrototype> BleedingAlert = "Bleed";
    }
}