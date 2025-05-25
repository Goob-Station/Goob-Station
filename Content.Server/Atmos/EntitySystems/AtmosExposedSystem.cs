// SPDX-FileCopyrightText: 2021 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Robust.Shared.Map;

namespace Content.Server.Atmos.EntitySystems
{
    /* doesn't seem to be a use for this at the moment, so it's disabled
    public sealed class AtmosExposedSystem : EntitySystem
    {}
    */

    [ByRefEvent]
    public readonly struct AtmosExposedUpdateEvent
    {
        public readonly EntityCoordinates Coordinates;
        public readonly GasMixture GasMixture;
        public readonly TransformComponent Transform;

        public AtmosExposedUpdateEvent(EntityCoordinates coordinates, GasMixture mixture, TransformComponent transform)
        {
            Coordinates = coordinates;
            GasMixture = mixture;
            Transform = transform;
        }
    }

    /// <summary>
    ///     Event that tries to query the mixture a certain entity is exposed to.
    ///     This is mainly intended for use with entities inside of containers.
    ///     This event is not raised for entities that are directly parented to the grid.
    /// </summary>
    [ByRefEvent]
    public struct AtmosExposedGetAirEvent
    {
        /// <summary>
        ///     The entity we want to query this for.
        /// </summary>
        public readonly Entity<TransformComponent> Entity;

        /// <summary>
        ///     The mixture that the entity is exposed to. Output parameter.
        /// </summary>
        public GasMixture? Gas = null;

        /// <summary>
        ///     Whether to excite the mixture, if possible.
        /// </summary>
        public readonly bool Excite = false;

        /// <summary>
        ///     Whether this event has been handled or not.
        ///     Check this before changing anything.
        /// </summary>
        public bool Handled = false;

        public AtmosExposedGetAirEvent(Entity<TransformComponent> entity, bool excite = false)
        {
            Entity = entity;
            Excite = excite;
        }
    }
}