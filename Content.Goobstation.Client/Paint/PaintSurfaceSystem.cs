using Content.Goobstation.Shared.Paint;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Goobstation.Client.Paint;

public sealed partial class PaintSurfaceSystem : SharedPaintSurfaceSystem
{
    [Dependency] private IClyde _clyde = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private readonly Dictionary<EntityUid, OwnedTexture> _textures = new();
    private readonly Rgba32[] _scratch = new Rgba32[PaintChunks.ChunkSize * PaintChunks.ChunkSize];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaintSurfaceComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PaintSurfaceComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PaintSurfaceComponent, PaintSurfaceUpdatedEvent>(OnUpdated);
    }

    public Texture? GetTexture(EntityUid uid)
    {
        return _textures.GetValueOrDefault(uid);
    }

    private void OnStartup(Entity<PaintSurfaceComponent> ent, ref ComponentStartup args)
    {
        UpdateTexture(ent, null);
    }

    private void OnShutdown(Entity<PaintSurfaceComponent> ent, ref ComponentShutdown args)
    {
        if (_textures.Remove(ent.Owner, out var texture))
            texture.Dispose();
    }

    private void OnUpdated(Entity<PaintSurfaceComponent> ent, ref PaintSurfaceUpdatedEvent args)
    {
        if (ent.Comp.LifeStage < ComponentLifeStage.Starting)
            return;

        UpdateTexture(ent, args.Chunks);

        if (_ui.TryGetOpenUi<PaintBoundUserInterface>(ent.Owner, PaintUiKey.Key, out var bui))
            bui.SurfaceUpdated(args.Chunks);
    }

    private void UpdateTexture(Entity<PaintSurfaceComponent> ent, List<int>? chunks)
    {
        if (ent.Comp.Image is not { } image)
            return;

        if (!_textures.TryGetValue(ent.Owner, out var texture) || texture.Size != image.Size)
        {
            texture?.Dispose();
            texture = PaintTextures.Create(_clyde, image, $"paint-surface-{ent.Owner}");
            _textures[ent.Owner] = texture;

            var ev = new PaintTextureChangedEvent();
            RaiseLocalEvent(ent, ref ev);
        }
        else if (chunks == null)
        {
            var count = PaintChunks.Count(image.Size);
            for (var i = 0; i < count; i++)
            {
                PaintTextures.UploadChunk(texture, image, i, _scratch);
            }
        }
        else
        {
            foreach (var chunk in chunks)
            {
                PaintTextures.UploadChunk(texture, image, chunk, _scratch);
            }
        }

        if (_sprite.TryGetLayer(ent.Owner, ent.Comp.Layer, out var layer, false) && layer.Texture != texture)
            _sprite.LayerSetTexture(layer, texture);
    }
}

[ByRefEvent]
public readonly record struct PaintTextureChangedEvent;
