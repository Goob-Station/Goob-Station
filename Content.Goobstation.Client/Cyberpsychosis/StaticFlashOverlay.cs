using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Overlays;

public sealed class StaticFlashOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private ShaderInstance _shader;
    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public Action? OnFinished;
    public TimeSpan Start = TimeSpan.Zero;
    public TimeSpan End = TimeSpan.Zero;
    private bool _finished = false;

    public StaticFlashOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = 102;

        _shader = _proto.Index<ShaderPrototype>("AdvancedStatic").InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var duration = End - Start;
        var current = _timing.CurTime - Start;

        var amount = current.TotalSeconds / duration.TotalSeconds;

        _shader.SetParameter("mixAmount", 1 - Math.Clamp((float) amount, 0, 1));
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var handle = args.WorldHandle;
        var viewport = args.WorldBounds;

        handle.UseShader(_shader);
        handle.DrawRect(viewport, Color.FromHex("#ffffffff"));
        handle.UseShader(null);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_timing.CurTime >= End && !_finished)
        {
            _finished = true;
            OnFinished?.Invoke();
        }
    }
}
