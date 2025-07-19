using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Content.Shared.Shadowkin;

namespace Content.Client.Overlays;

/// <summary>
///     A simple overlay that applies a colored tint to the screen.
/// </summary>
public sealed class ColorTintOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] IEntityManager _entityManager = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _shader;

    /// <summary>
    ///     The color to tint the screen to as RGB on a scale of 0-1.
    /// </summary>
    public Robust.Shared.Maths.Vector3? TintColor = null;
    /// <summary>
    ///     The percent to tint the screen by on a scale of 0-1.
    /// </summary>
    public float? TintAmount = null;

    public ColorTintOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototype.Index<ShaderPrototype>("ColorTint").InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { Valid: true } player
            || !_entityManager.HasComponent<ShadowkinComponent>(player))
            return false;

        return base.BeforeDraw(in args);
    }
    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null)
            return;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        if (TintColor != null)
            _shader.SetParameter("tint_color", (Robust.Shared.Maths.Vector3) TintColor);
        if (TintAmount != null)
            _shader.SetParameter("tint_amount", (float) TintAmount);

        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;
        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(_shader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
    }
}
