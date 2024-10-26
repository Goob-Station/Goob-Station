using Content.Shared._White.Overlays;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._White.Overlays;

public class BaseSwitchableOverlay<TComp> : Overlay
    where TComp : SwitchableOverlayComponent
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _shader;

    public BaseSwitchableOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototype.Index<ShaderPrototype>("NightVision").Instance().Duplicate();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null
            || _player.LocalEntity == null
            || !_entity.TryGetComponent<TComp>(_player.LocalEntity.Value, out var component)
            || !component.IsActive)
            return;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("tint", component.Tint);
        _shader.SetParameter("luminance_threshold", component.Strength);
        _shader.SetParameter("noise_amount", component.Noise);

        var worldHandle = args.WorldHandle;

        worldHandle.SetTransform(Matrix3.Identity);
        worldHandle.UseShader(_shader);
        worldHandle.DrawRect(args.WorldBounds, component.Color);
        worldHandle.UseShader(null);
    }
}
