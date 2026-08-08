using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Old horror style overlay shown to slashers as they incorporealize.
/// </summary>
public sealed class SlasherIncorporealOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherJaunt";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;

    private const float FadeSpeed = 2.2f;

    private float _intensity;

    public SlasherIncorporealOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _proto.Index(Shader).InstanceUnique();
    }

    private bool LocalPlayerIncorporeal =>
        _entMan.TryGetComponent(_player.LocalEntity, out SlasherIncorporealComponent? comp)
        && comp.IsIncorporeal;

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entMan.TryGetComponent(_player.LocalEntity, out EyeComponent? eyeComp)
            || args.Viewport.Eye != eyeComp.Eye)
            return false;

        var target = LocalPlayerIncorporeal ? 1f : 0f;
        var dt = (float) _timing.FrameTime.TotalSeconds * FadeSpeed;
        _intensity = Math.Clamp(_intensity + Math.Sign(target - _intensity) * dt, 0f, 1f);
        if (Math.Abs(target - _intensity) < dt)
            _intensity = target;

        return _intensity > 0f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("intensity", _intensity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
