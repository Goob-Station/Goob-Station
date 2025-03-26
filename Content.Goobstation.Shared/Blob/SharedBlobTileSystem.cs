using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Verbs;

namespace Content.Shared._Goobstation.Blob;

public abstract class SharedBlobTileSystem : EntitySystem
{
    protected EntityQuery<BlobObserverComponent> ObserverQuery;
    protected EntityQuery<BlobCoreComponent> CoreQuery;
    protected EntityQuery<TransformComponent> TransformQuery;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobTileComponent, GetVerbsEvent<AlternativeVerb>>(AddUpgradeVerb);

        ObserverQuery = GetEntityQuery<BlobObserverComponent>();
        CoreQuery = GetEntityQuery<BlobCoreComponent>();
        TransformQuery = GetEntityQuery<TransformComponent>();
    }

    protected abstract void TryUpgrade(Entity<BlobTileComponent> target, Entity<BlobCoreComponent> core, EntityUid observer);

    private void AddUpgradeVerb(EntityUid uid, BlobTileComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!ObserverQuery.TryGetComponent(args.User, out var ghostBlobComponent))
            return;

        if (ghostBlobComponent.Core == null ||
            component.Core == null ||
            !CoreQuery.HasComponent(ghostBlobComponent.Core.Value))
            return;

        if (TransformQuery.TryGetComponent(uid, out var transformComponent) && !transformComponent.Anchored)
            return;

        var verbName = component.BlobTileType switch
        {
            BlobTileType.Normal => Loc.GetString("blob-verb-upgrade-to-strong"),
            BlobTileType.Strong => Loc.GetString("blob-verb-upgrade-to-reflective"),
            _ => Loc.GetString("blob-verb-upgrade")
        };

        AlternativeVerb verb = new()
        {
            Act = () => TryUpgrade((uid, component), ghostBlobComponent.Core.Value, args.User),
            Text = verbName
        };
        args.Verbs.Add(verb);
    }
}
