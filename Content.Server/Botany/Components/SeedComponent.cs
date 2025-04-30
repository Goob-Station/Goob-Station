// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flesh <62557990+PolterTzi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Magnus Larsen <i.am.larsenml@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Botany.Systems;
using Content.Shared.Botany.Components;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Botany.Components
{
    [RegisterComponent, Access(typeof(BotanySystem))]
    public sealed partial class SeedComponent : SharedSeedComponent
    {
        /// <summary>
        ///     Seed data containing information about the plant type & properties that this seed can grow seed. If
        ///     null, will instead attempt to get data from a seed prototype, if one is defined. See <see
        ///     cref="SeedId"/>.
        /// </summary>
        [DataField("seed")]
        public SeedData? Seed;

        /// <summary>
        ///     If not null, overrides the plant's initial health. Otherwise, the plant's initial health is set to the Endurance value.
        /// </summary>
        [DataField]
        public float? HealthOverride = null;

        /// <summary>
        ///     Name of a base seed prototype that is used if <see cref="Seed"/> is null.
        /// </summary>
        [DataField("seedId", customTypeSerializer: typeof(PrototypeIdSerializer<SeedPrototype>))]
        public string? SeedId;
    }
}