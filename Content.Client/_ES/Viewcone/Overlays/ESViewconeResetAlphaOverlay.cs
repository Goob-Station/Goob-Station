using Content.Shared._ES.Viewcone;
using Content.Shared.Clothing.Components;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Graphics;

namespace Content.Client._ES.Viewcone.Overlays;

public sealed class ESViewconeResetAlphaOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _ent = default!;
    private readonly ESViewconeSystem _cone;
    private readonly SpriteSystem _sprite;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public ESViewconeResetAlphaOverlay()
    {
        _cone = _ent.EntitySysManager.GetEntitySystem<ESViewconeSystem>();
        _sprite = _ent.EntitySysManager.GetEntitySystem<SpriteSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        foreach (var (ent, baseAlpha) in _cone.CachedOccludables)
        {
            _sprite.SetColor(ent!, ent.Comp.Color.WithAlpha(baseAlpha));
        }
    }
}

