// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Justin Trotter <trotter.justin@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Guidebook;
using Robust.Shared.Audio;

namespace Content.Server.Atmos.Piping.Unary.Components
{
    [RegisterComponent]
    public sealed partial class GasCanisterComponent : Component, IGasMixtureHolder
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("port")]
        public string PortName { get; set; } = "port";

        /// <summary>
        ///     Container name for the gas tank holder.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("container")]
        public string ContainerName { get; set; } = "tank_slot";

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public ItemSlot GasTankSlot = new();

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("gasMixture")]
        public GasMixture Air { get; set; } = new();

        /// <summary>
        ///     Last recorded pressure, for appearance-updating purposes.
        /// </summary>
        public float LastPressure { get; set; } = 0f;

        /// <summary>
        ///     Minimum release pressure possible for the release valve.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("minReleasePressure")]
        public float MinReleasePressure { get; set; } = Atmospherics.OneAtmosphere / 10;

        /// <summary>
        ///     Maximum release pressure possible for the release valve.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("maxReleasePressure")]
        public float MaxReleasePressure { get; set; } = Atmospherics.OneAtmosphere * 10;

        /// <summary>
        ///     Valve release pressure.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("releasePressure")]
        public float ReleasePressure { get; set; } = Atmospherics.OneAtmosphere;

        /// <summary>
        ///     Whether the release valve is open on the canister.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("releaseValve")]
        public bool ReleaseValve { get; set; } = false;

        [DataField("accessDeniedSound")]
        public SoundSpecifier AccessDeniedSound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");

        #region GuidebookData

        [GuidebookData]
        public float Volume => Air.Volume;

        #endregion
    }
}