// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Server.Chemistry.TileReactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class SpillIfPuddlePresentTileReaction : ITileReaction
    {
        public FixedPoint2 TileReact(TileRef tile,
            ReagentPrototype reagent,
            FixedPoint2 reactVolume,
            IEntityManager entityManager,
            List<ReagentData>? data)
        {
            var spillSystem = entityManager.System<PuddleSystem>();
            if (reactVolume < 5 || !spillSystem.TryGetPuddle(tile, out _))
                return FixedPoint2.Zero;

            return spillSystem.TrySpillAt(tile, new Solution(reagent.ID, reactVolume, data), out _, sound: false, tileReact: false)
                ? reactVolume
                : FixedPoint2.Zero;
        }
    }
}