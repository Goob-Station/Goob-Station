// SPDX-FileCopyrightText: 2020 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 DTanxxx <55208219+DTanxxx@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Morshu32 <paulbisaccia@live.it>
// SPDX-FileCopyrightText: 2020 Vince <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 V�ctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 V�ctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Absolute-Potato <jamesgamesmahar@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 K-Dynamic <20566341+K-Dynamic@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Myra <vascreeper@yahoo.com>
// SPDX-FileCopyrightText: 2025 Zachary Higgs <compgeek223@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StepTrigger.Components;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Slippery
{
    /// <summary>
    /// Causes somebody to slip when they walk over this entity.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="StepTriggerComponent"/>, see that component for some additional properties.
    /// </remarks>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class SlipperyComponent : Component
    {
        /// <summary>
        /// Path to the sound to be played when a mob slips.
        /// </summary>
        [DataField, AutoNetworkedField]
        [Access(Other = AccessPermissions.ReadWriteExecute)]
        public SoundSpecifier SlipSound = new SoundPathSpecifier("/Audio/Effects/slip.ogg");

        /// <summary>
        /// Loads the data needed to determine how slippery something is.
        /// </summary>
        [DataField, AutoNetworkedField]
        public SlipperyEffectEntry SlipData = new();
    }
    /// <summary>
    /// Stores the data for slipperiness that way reagents and this component can use it.
    /// </summary>
    [DataDefinition, Serializable, NetSerializable]
    public sealed partial class SlipperyEffectEntry
    {
        /// <summary>
        /// How many seconds the mob will be paralyzed for.
        /// </summary>
        [DataField]
        public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(1.5);

        /// <summary>
        /// The entity's speed will be multiplied by this to slip it forwards.
        /// </summary>
        [DataField]
        public float LaunchForwardsMultiplier = 1.5f;

        /// <summary>
        /// Minimum speed entity must be moving to slip.
        /// </summary>
        [DataField]
        public float RequiredSlipSpeed = 3.5f;

        /// <summary>
        /// If this is true, any slipping entity loses its friction until
        /// it's not colliding with any SuperSlippery entities anymore.
        /// They also will fail any attempts to stand up unless they have no-slips.
        /// </summary>
        [DataField]
        public bool SuperSlippery;


        /// <summary>
        /// Goobstation.
        /// Whether we should slip on step.
        /// </summary>
        [DataField]
        [Access(Other = AccessPermissions.ReadWrite)]
        public bool SlipOnStep = true;

        /// <summary>
        /// This is used to store the friction modifier that is used on a sliding entity.
        /// </summary>
        [DataField]
        public float SlipFriction;
    }
}
