// SPDX-FileCopyrightText: 2020 Campbell Suter <znix@znix.xyz>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 silicons <2003111+silicons@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Atmos.Components
{
    [RegisterComponent, Access(typeof(AirtightSystem))]
    public sealed partial class AirtightComponent : Component
    {
        public (EntityUid Grid, Vector2i Tile) LastPosition { get; set; }

        /// <summary>
        /// The directions in which this entity should block airflow, relative to its own reference frame.
        /// </summary>
        [DataField("airBlockedDirection", customTypeSerializer: typeof(FlagSerializer<AtmosDirectionFlags>))]
        public int InitialAirBlockedDirection { get; set; } = (int) AtmosDirection.All;

        /// <summary>
        /// The directions in which the entity is currently blocking airflow, relative to the grid that the entity is on.
        /// I.e., this is a variant of <see cref="InitialAirBlockedDirection"/> that takes into account the entity's
        /// current rotation.
        /// </summary>
        [ViewVariables]
        public int CurrentAirBlockedDirection;

        /// <summary>
        /// Whether the airtight entity is currently blocking airflow.
        /// </summary>
        [DataField]
        public bool AirBlocked { get; set; } = true;

        /// <summary>
        /// If true, entities on this tile will attempt to draw air from surrounding tiles when they become unblocked
        /// and currently have no air. This is generally only required when <see cref="NoAirWhenFullyAirBlocked"/> is
        /// true, or if the entity is likely to occupy the same tile as another no-air airtight entity.
        /// </summary>
        [DataField]
        public bool FixVacuum { get; set; } = true;
        // I think fixvacuum exists to ensure that repeatedly closing/opening air-blocking doors doesn't end up
        // depressurizing a room. However it can also effectively be used as a means of generating gasses for free
        // TODO ATMOS Mass conservation. Make it actually push/pull air from adjacent tiles instead of destroying & creating,


        // TODO ATMOS Do we need these two fields?
        [DataField("rotateAirBlocked")]
        public bool RotateAirBlocked { get; set; } = true;

        // TODO ATMOS remove this? What is this even for??
        [DataField("fixAirBlockedDirectionInitialize")]
        public bool FixAirBlockedDirectionInitialize { get; set; } = true;

        /// <summary>
        /// If true, then the tile that this entity is on will have no air at all if all directions are blocked.
        /// </summary>
        [DataField]
        public bool NoAirWhenFullyAirBlocked { get; set; } = true;

        /// <inheritdoc cref="CurrentAirBlockedDirection"/>
        [Access(Other = AccessPermissions.ReadWriteExecute)]
        public AtmosDirection AirBlockedDirection => (AtmosDirection)CurrentAirBlockedDirection;
    }
}