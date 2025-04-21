using Robust.Shared.Containers;
using Content.Goobstation.Shared.Porg.Components;

namespace Content.Goobstation.Shared.Porg
{
    public sealed class PorgNamingSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PorgNamingComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
            SubscribeLocalEvent<PorgNamingComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
        }

        private void OnItemInserted(EntityUid uid, PorgNamingComponent component, EntInsertedIntoContainerMessage args)
        {
            string? entityName = null;
            if (HasComp<PorgNamerComponent>(args.Entity))
            {
                entityName = $"({Name(args.Entity)})";
            }
            UpdateContainerName(args.Container.Owner, entityName);
        }
        private void OnItemRemoved(EntityUid uid, PorgNamingComponent naming, ref EntRemovedFromContainerMessage args)
        {
            string? entityName = null;
            UpdateContainerName(args.Container.Owner, entityName);
        }
        private void UpdateContainerName(EntityUid containerUid, string? entityName)
        {
            _metaData.SetEntityName(containerUid, $"pOrg {entityName}");
        }
    }
}
