using Content.Goobstation.Shared.ModSuits.Modules;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModDampenedProjectileVisualsSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    private static readonly Color DampenedColor = Color.FromHex("#8fd6ffaa");

    private readonly Dictionary<EntityUid, Color> _tinted = new();
    private readonly List<EntityUid> _stale = new();

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<ModDampenedProjectileComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out _, out var sprite))
        {
            if (_tinted.ContainsKey(uid))
                continue;

            _tinted[uid] = sprite.Color;
            _sprite.SetColor((uid, sprite), DampenedColor);
        }

        _stale.Clear();
        foreach (var uid in _tinted.Keys)
        {
            if (!HasComp<ModDampenedProjectileComponent>(uid))
                _stale.Add(uid);
        }

        foreach (var uid in _stale)
        {
            if (_spriteQuery.TryComp(uid, out var sprite))
                _sprite.SetColor((uid, sprite), _tinted[uid]);

            _tinted.Remove(uid);
        }
    }
}
