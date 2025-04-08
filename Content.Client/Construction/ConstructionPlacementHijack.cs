// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2019 ZelteHonor <gabrieldionbouchard@gmail.com>
// SPDX-FileCopyrightText: 2020 Campbell Suter <znix@znix.xyz>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 silicons <2003111+silicons@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2023 08A <git@08a.re>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Shared.Construction.Prototypes;
using Robust.Client.Placement;
using Robust.Client.Utility;
using Robust.Shared.Map;

namespace Content.Client.Construction
{
    public sealed class ConstructionPlacementHijack : PlacementHijack
    {
        private readonly ConstructionSystem _constructionSystem;
        private readonly ConstructionPrototype? _prototype;

        public override bool CanRotate { get; }

        public ConstructionPlacementHijack(ConstructionSystem constructionSystem, ConstructionPrototype? prototype)
        {
            _constructionSystem = constructionSystem;
            _prototype = prototype;
            CanRotate = prototype?.CanRotate ?? true;
        }

        /// <inheritdoc />
        public override bool HijackPlacementRequest(EntityCoordinates coordinates)
        {
            if (_prototype != null)
            {
                var dir = Manager.Direction;
                _constructionSystem.SpawnGhost(_prototype, coordinates, dir);
            }
            return true;
        }

        /// <inheritdoc />
        public override bool HijackDeletion(EntityUid entity)
        {
            if (IoCManager.Resolve<IEntityManager>().HasComponent<ConstructionGhostComponent>(entity))
            {
                _constructionSystem.ClearGhost(entity.GetHashCode());
            }
            return true;
        }

        /// <inheritdoc />
        public override void StartHijack(PlacementManager manager)
        {
            base.StartHijack(manager);
            manager.CurrentTextures = _prototype?.Layers.Select(sprite => sprite.DirFrame0()).ToList();
        }
    }
}