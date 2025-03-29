using System.Linq;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Blob.Chemistry;

public sealed class BlobSmokeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Shared.Blob.Chemistry.BlobSmokeColorComponent, AfterAutoHandleStateEvent>(OnBlobTileHandleState);
    }

    private void OnBlobTileHandleState(EntityUid uid, Shared.Blob.Chemistry.BlobSmokeColorComponent component, ref AfterAutoHandleStateEvent state)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            sprite.LayerSetColor(i, component.Color);
        }
    }
}
