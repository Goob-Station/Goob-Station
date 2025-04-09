// SPDX-FileCopyrightText: 2021 Daniel Castro Razo <eldanielcr@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 08A <git@08a.re>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.IdentityManagement;
using Robust.Client.GameObjects;
using System.Linq;

namespace Content.Client.ContextMenu.UI
{
    public sealed partial class EntityMenuUIController
    {
        public const int GroupingTypesCount = 2;
        private int GroupingContextMenuType { get; set; }
        public void OnGroupingChanged(int obj)
        {
            _context.Close();
            GroupingContextMenuType = obj;
        }

        private List<List<EntityUid>> GroupEntities(IEnumerable<EntityUid> entities, int depth = 0)
        {
            if (GroupingContextMenuType == 0)
            {
                var newEntities = entities.GroupBy(e => Identity.Name(e, _entityManager)).ToList();
                return newEntities.Select(grp => grp.ToList()).ToList();
            }
            else
            {
                var newEntities = entities.GroupBy(e => e, new PrototypeAndStatesContextMenuComparer(depth, _entityManager)).ToList();
                return newEntities.Select(grp => grp.ToList()).ToList();
            }
        }

        private sealed class PrototypeAndStatesContextMenuComparer : IEqualityComparer<EntityUid>
        {
            private static readonly List<Func<EntityUid, EntityUid, IEntityManager, bool>> EqualsList = new()
            {
                (a, b, entMan) => entMan.GetComponent<MetaDataComponent>(a).EntityPrototype!.ID == entMan.GetComponent<MetaDataComponent>(b).EntityPrototype!.ID,
                (a, b, entMan) =>
                {
                    entMan.TryGetComponent(a, out SpriteComponent? spriteA);
                    entMan.TryGetComponent(b, out SpriteComponent? spriteB);

                    if (spriteA == null || spriteB == null)
                        return spriteA == spriteB;

                    var xStates = spriteA.AllLayers.Where(e => e.Visible).Select(s => s.RsiState.Name);
                    var yStates = spriteB.AllLayers.Where(e => e.Visible).Select(s => s.RsiState.Name);

                    return xStates.OrderBy(t => t).SequenceEqual(yStates.OrderBy(t => t));
                },
            };
            private static readonly List<Func<EntityUid, IEntityManager, int>> GetHashCodeList = new()
            {
                (e, entMan) => EqualityComparer<string>.Default.GetHashCode(entMan.GetComponent<MetaDataComponent>(e).EntityPrototype!.ID),
                (e, entMan) =>
                {
                    var hash = 0;
                    foreach (var element in entMan.GetComponent<SpriteComponent>(e).AllLayers.Where(obj => obj.Visible).Select(s => s.RsiState.Name))
                    {
                        hash ^= EqualityComparer<string>.Default.GetHashCode(element!);
                    }
                    return hash;
                },
            };

            private static int Count => EqualsList.Count - 1;

            private readonly int _depth;
            private readonly IEntityManager _entMan;
            public PrototypeAndStatesContextMenuComparer(int step = 0, IEntityManager? entMan = null)
            {
                IoCManager.Resolve(ref entMan);

                _depth = step > Count ? Count : step;
                _entMan = entMan;
            }

            public bool Equals(EntityUid x, EntityUid y)
            {
                if (x == default)
                {
                    return y == default;
                }

                return y != default && EqualsList[_depth](x, y, _entMan);
            }

            public int GetHashCode(EntityUid e)
            {
                return GetHashCodeList[_depth](e, _entMan);
            }
        }
    }
}