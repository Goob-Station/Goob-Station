// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction;
using JetBrains.Annotations;
using Content.Shared.Examine;
using Content.Shared.Tag;

namespace Content.Server.Construction.Conditions
{
    /// <summary>
    ///     This condition checks whether if an entity with the <see cref="TagComponent"/> possesses a specific tag
    /// </summary>
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class HasTag : IGraphCondition
    {
        /// <summary>
        ///     The tag the entity is being checked for
        /// </summary>
        [DataField("tag")]
        public string Tag { get; private set; }

        public bool Condition(EntityUid uid, IEntityManager entityManager)
        {
            if (!entityManager.TrySystem<TagSystem>(out var tagSystem))
                return false;

            return tagSystem.HasTag(uid, Tag);
        }

        public bool DoExamine(ExaminedEvent args)
        {
            return false;
        }

        public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
        {
            yield return new ConstructionGuideEntry()
            {
            };
        }
    }
}