using Content.Goobstation.Shared.Blob.Components;
using Content.Shared.Destructible;

namespace Content.Goobstation.Server.Blob.Systems;

public sealed class BlobStorageSystem : EntitySystem
{
    [Dependency] private readonly BlobCoreSystem _blobCore = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobStorageComponent, BlobTransformTileEvent>(OnTransformed);
        SubscribeLocalEvent<BlobStorageComponent, DestructionEventArgs>(KillStorage);
        SubscribeLocalEvent<BlobStorageComponent, EntityTerminatingEvent>(KillStorage);
    }

    private void OnTransformed(Entity<BlobStorageComponent> ent, ref BlobTransformTileEvent args)
    {
        if (!TryComp<BlobTileComponent>(ent, out var tileComp) ||
            tileComp.Core == null ||
            TerminatingOrDeleted(tileComp.Core.Value.Owner))
            return;

        tileComp.Core.Value.Comp.MaxStorageAmount += ent.Comp.AddTotalStorage;
    }

    private void KillStorage<T>(EntityUid uid, BlobStorageComponent component, T args)
    {
        if (!TryComp<BlobTileComponent>(uid, out var tileComp) ||
            tileComp.Core == null ||
            TerminatingOrDeleted(tileComp.Core.Value.Owner))
            return;

        tileComp.Core.Value.Comp.MaxStorageAmount -= component.AddTotalStorage;
        _blobCore.ChangeBlobPoint(tileComp.Core.Value, -component.DeleteOnRemove);
    }
}
