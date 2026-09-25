using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Slasher.Overlays;

public sealed class SlasherRegenerateOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> RegenerateShader = "SlasherRegenerate";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;
    private readonly EntityQuery<SlasherRegenerateOverlayComponent> _overlayQuery;
    private readonly EntityQuery<EyeComponent> _eyeQuery;

    public SlasherRegenerateOverlay()
    {
        IoCManager.InjectDependencies(this);

        _shader = _proto.Index(RegenerateShader).InstanceUnique();
        _overlayQuery = _entMan.GetEntityQuery<SlasherRegenerateOverlayComponent>();
        _eyeQuery = _entMan.GetEntityQuery<EyeComponent>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null
            || !_overlayQuery.TryComp(_player.LocalEntity, out var overlay)
            || !_eyeQuery.TryComp(_player.LocalEntity, out var eye)
            || args.Viewport.Eye != eye.Eye)
            return;

        var start = overlay.EndTime - overlay.Duration;
        var progress = (float) ((_timing.CurTime - start).TotalSeconds / overlay.Duration.TotalSeconds);
        if (progress is < 0f or >= 1f)
            return;

        var handle = args.WorldHandle;
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("progress", progress);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
