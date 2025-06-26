// SPDX-FileCopyrightText: 2020 Campbell Suter <znix@znix.xyz>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 DTanxxx <55208219+DTanxxx@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 a.rudenko <creadth@gmail.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2020 creadth <creadth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 silicons <2003111+silicons@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Bright0 <55061890+Bright0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 liltenhead <104418166+liltenhead@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class GasTankComponent : Component, IGasMixtureHolder
    {
        private const float DefaultLowPressure = 0f;
        private const float DefaultOutputPressure = Atmospherics.OneAtmosphere;

        public int Integrity = 3;
        public bool IsLowPressure => (Air?.Pressure ?? 0F) <= TankLowPressure;

        [DataField]
        public float? MaxExplosionRange; // Goobstation - If null, use the atmos explosion range cvar, otherwise, use this value

        [ViewVariables(VVAccess.ReadWrite), DataField("ruptureSound")]
        public SoundSpecifier RuptureSound = new SoundPathSpecifier("/Audio/Effects/spray.ogg");

        [ViewVariables(VVAccess.ReadWrite), DataField("connectSound")]
        public SoundSpecifier? ConnectSound =
            new SoundPathSpecifier("/Audio/Effects/internals.ogg")
            {
                Params = AudioParams.Default.WithVolume(5f),
            };

        [ViewVariables(VVAccess.ReadWrite), DataField("disconnectSound")]
        public SoundSpecifier? DisconnectSound;

        // Cancel toggles sounds if we re-toggle again.

        public EntityUid? ConnectStream;
        public EntityUid? DisconnectStream;

        [DataField("air"), ViewVariables(VVAccess.ReadWrite)]
        public GasMixture Air { get; set; } = new();

        /// <summary>
        ///     Pressure at which tank should be considered 'low' such as for internals.
        /// </summary>
        [DataField("tankLowPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float TankLowPressure = DefaultLowPressure;

        /// <summary>
        ///     Distributed pressure.
        /// </summary>
        [DataField("outputPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float OutputPressure = DefaultOutputPressure;

        /// <summary>
        ///     The maximum allowed output pressure.
        /// </summary>
        [DataField("maxOutputPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float MaxOutputPressure = 3 * DefaultOutputPressure;

        /// <summary>
        ///     Tank is connected to internals.
        /// </summary>
        [ViewVariables]
        public bool IsConnected => User != null;

        [ViewVariables]
        public EntityUid? User;

        /// <summary>
        ///     True if this entity was recently moved out of a container. This might have been a hand -> inventory
        ///     transfer, or it might have been the user dropping the tank. This indicates the tank needs to be checked.
        /// </summary>
        [ViewVariables]
        public bool CheckUser;

        /// <summary>
        ///     Pressure at which tanks start leaking.
        /// </summary>
        [DataField("tankLeakPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float TankLeakPressure = 30 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Pressure at which tank spills all contents into atmosphere.
        /// </summary>
        [DataField("tankRupturePressure"), ViewVariables(VVAccess.ReadWrite)]
        public float TankRupturePressure = 40 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Base 3x3 explosion.
        /// </summary>
        [DataField("tankFragmentPressure"), ViewVariables(VVAccess.ReadWrite)]
        public float TankFragmentPressure = 50 * Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Increases explosion for each scale kPa above threshold.
        /// </summary>
        [DataField("tankFragmentScale"), ViewVariables(VVAccess.ReadWrite)]
        public float TankFragmentScale = 2 * Atmospherics.OneAtmosphere;

        [DataField("toggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ToggleAction = "ActionToggleInternals";

        [DataField("toggleActionEntity")] public EntityUid? ToggleActionEntity;

        /// <summary>
        ///     Valve to release gas from tank
        /// </summary>
        [DataField("isValveOpen"), ViewVariables(VVAccess.ReadWrite)]
        public bool IsValveOpen = false;

        /// <summary>
        ///     Gas release rate in L/s
        /// </summary>
        [DataField("valveOutputRate"), ViewVariables(VVAccess.ReadWrite)]
        public float ValveOutputRate = 100f;

        [DataField("valveSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier ValveSound =
            new SoundCollectionSpecifier("valveSqueak")
            {
                Params = AudioParams.Default.WithVolume(-5f),
            };
    }
}