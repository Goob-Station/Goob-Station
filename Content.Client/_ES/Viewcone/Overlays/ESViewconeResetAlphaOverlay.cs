using Content.Shared._ES.Viewcone;
using Content.Shared.Clothing.Components;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Graphics;

namespace Content.Client._ES.Viewcone.Overlays;

/// <summary>
///     After <see cref="ESViewconeSetAlphaOverlay"/> has run, resets the alpha of affected entities
///     back to normal.
/// </summary>
public sealed class ESViewconeResetAlphaOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _ent = default!;
    private readonly ESViewconeOverlayManagementSystem _cone;
    private readonly SpriteSystem _sprite;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public ESViewconeResetAlphaOverlay()
    {
        IoCManager.InjectDependencies(this);

        _cone = _ent.EntitySysManager.GetEntitySystem<ESViewconeOverlayManagementSystem>();
        _sprite = _ent.EntitySysManager.GetEntitySystem<SpriteSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        foreach (var (ent, baseAlpha) in _cone.CachedBaseAlphas)
        {
            _sprite.SetColor(ent!, ent.Comp.Color.WithAlpha(baseAlpha));
        }

        _cone.CachedBaseAlphas.Clear();
    }
}

