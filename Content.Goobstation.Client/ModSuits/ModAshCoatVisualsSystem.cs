using Content.Goobstation.Shared.ModSuits.Modules;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModAshCoatVisualsSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private EntityQuery<ModAshCoatComponent> _coatQuery = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    private const string LayerKey = "mod-ash-coat";

    private static readonly SpriteSpecifier.Rsi Coat =
        new(new ResPath("_Goobstation/Clothing/OuterClothing/Modsuits/mining.rsi"), "ash-coat");

    private readonly HashSet<EntityUid> _coated = new();
    private readonly List<EntityUid> _stale = new();

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<ModAshCoatComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var coat, out var sprite))
        {
            var alpha = coat.MaxLayers > 0 ? Math.Clamp((float) coat.Layers / coat.MaxLayers, 0f, 1f) : 0f;
            var ent = (uid, sprite);

            if (!_sprite.LayerMapTryGet(ent, LayerKey, out var index, false))
            {
                if (alpha <= 0f)
                    continue;

                index = _sprite.AddLayer(ent, Coat);
                _sprite.LayerMapSet(ent, LayerKey, index);
                _coated.Add(uid);
            }

            _sprite.LayerSetVisible(ent, index, alpha > 0f);
            _sprite.LayerSetColor(ent, index, Color.White.WithAlpha(alpha));
        }

        _stale.Clear();
        foreach (var uid in _coated)
        {
            if (!_coatQuery.HasComp(uid))
                _stale.Add(uid);
        }

        foreach (var uid in _stale)
        {
            _coated.Remove(uid);
            if (_spriteQuery.TryComp(uid, out var sprite))
                _sprite.RemoveLayer((uid, sprite), LayerKey, false);
        }
    }
}
