using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Full-screen film-reel effect played for a moment when a nearby Slasher Regenerates.
/// </summary>
public sealed class SlasherRegenerateOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherRegenerate";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;

    private const float Duration = 3f;

    private float _elapsed = Duration;

    public SlasherRegenerateOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _proto.Index(Shader).InstanceUnique();
    }

    public void Trigger()
    {
        _elapsed = 0f;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        if (_elapsed < Duration)
            _elapsed += args.DeltaSeconds;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entMan.TryGetComponent(_player.LocalEntity, out EyeComponent? eyeComp)
            || args.Viewport.Eye != eyeComp.Eye)
            return false;

        return _elapsed < Duration;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("progress", _elapsed / Duration);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
