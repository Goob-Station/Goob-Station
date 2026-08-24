// SPDX-License-Identifier: MIT

using Content.Shared.Tag;
using Robust.Shared.Prototypes; // Goobstation

namespace Content.Shared.Construction.Steps
{
    [DataDefinition]
    public sealed partial class TagConstructionGraphStep : ArbitraryInsertConstructionGraphStep
    {
        [DataField(required: true)] // Goobstation - why was it no required
        private ProtoId<TagPrototype> Tag; // Goobstation - use ProtoId

        public override bool EntityValid(EntityUid uid, IEntityManager entityManager, IComponentFactory compFactory)
        {
            var tagSystem = entityManager.EntitySysManager.GetEntitySystem<TagSystem>();
            return tagSystem.HasTag(uid, Tag); // Goobstation - dont need null check anymore
        }
    }
}
