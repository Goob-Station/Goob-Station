using Content.Client.UserInterface.Systems;
using Content.Goobstation.Shared.GridShield;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.GridShield;

public sealed class GridShieldVisualizerSystem : VisualizerSystem<GridShieldVisualsComponent>
{
    private EntityQuery<GridShieldComponent> _shieldQuery;

    public override void Initialize()
    {
        base.Initialize();
        _shieldQuery = GetEntityQuery<GridShieldComponent>();
    }

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<GridShieldVisualsComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var visuals, out var sprite, out var xform))
        {
            if (!_shieldQuery.TryComp(xform.GridUid, out var shield))
                continue;

            var progress = shield.CurrentHealth;
            var curColor = ProgressColorSystem.InterpolateColorGaussian(new []{visuals.StartColor, visuals.EndColor}, progress);

            SpriteSystem.SetColor((uid, sprite), curColor.WithAlpha(sprite.Color.A));
        }
    }
}
