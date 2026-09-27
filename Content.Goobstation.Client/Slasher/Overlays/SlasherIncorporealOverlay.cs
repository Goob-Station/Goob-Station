using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Slasher.Overlays;

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
    private readonly EntityQuery<SlasherIncorporealOverlayComponent> _overlayQuery;
    private readonly EntityQuery<EyeComponent> _eyeQuery;

    public float Intensity;

    public SlasherIncorporealOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _proto.Index(Shader).InstanceUnique();
        _overlayQuery = _entMan.GetEntityQuery<SlasherIncorporealOverlayComponent>();
        _eyeQuery = _entMan.GetEntityQuery<EyeComponent>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_overlayQuery.TryComp(_player.LocalEntity, out var overlay)
            || !_eyeQuery.TryComp(_player.LocalEntity, out var eyeComp)
            || args.Viewport.Eye != eyeComp.Eye)
            return false;

        var dt = (float) _timing.FrameTime.TotalSeconds * overlay.FadeSpeed;
        Intensity = Math.Min(Intensity + dt, 1f);

        return Intensity > 0f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("intensity", Intensity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
