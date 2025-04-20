using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Content.Shared.IdentityManagement;
using Robust.Shared.IoC;
using Content.Shared.Containers;
using Content.Shared.ChangeNameInContainer;
using Content.Shared.Chat;
using Content.Shared.Speech;
using Content.Shared.Heretic;

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
            UpdateContainerName(args.Container.Owner, args.Entity);
        }

        private void OnItemRemoved(EntityUid uid, PorgNamingComponent naming, ref EntRemovedFromContainerMessage args)
        {
            UpdateContainerName(args.Container.Owner, null);
        }
        private void UpdateContainerName(EntityUid containerUid, EntityUid? containedUid)
        {
            string baseName;
            if (containedUid.HasValue && _entityManager.TryGetComponent<MetaDataComponent>(containedUid.Value, out var containedMetaData))
            {
                if (!_entityManager.HasComponent<PorgNamerComponent>(containedUid.Value))
                {
                    _metaData.SetEntityName(containerUid, "pOrg");
                }
                else
                {
                    baseName = containedMetaData.EntityName;
                    _metaData.SetEntityName(containerUid, $"pOrg ({baseName})");
                }
            }
            else
            {
                _metaData.SetEntityName(containerUid, "pOrg");
            }
        }
    }
    [RegisterComponent]
    public sealed partial class PorgNamingComponent : Component
    {
    }
    [RegisterComponent]
    public sealed partial class PorgNamerComponent: Component
    {
    }
}
