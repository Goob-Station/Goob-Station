using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Handles the slasher fear overlay.
/// </summary>
public sealed class SlasherFearOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> FearShader = "SlasherFear";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;
    private readonly EntityQuery<SlasherFearOverlayComponent> _fearQuery;
    private readonly EntityQuery<EyeComponent> _eyeQuery;

    public float Intensity;

    public SlasherFearOverlay()
    {
        IoCManager.InjectDependencies(this);

        _shader = _proto.Index(FearShader).InstanceUnique();
        _fearQuery = _entMan.GetEntityQuery<SlasherFearOverlayComponent>();
        _eyeQuery = _entMan.GetEntityQuery<EyeComponent>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null
            || Intensity <= 0.001f
            || !_fearQuery.HasComp(_player.LocalEntity)
            || !_eyeQuery.TryComp(_player.LocalEntity, out var eye)
            || args.Viewport.Eye != eye.Eye)
            return;

        var handle = args.WorldHandle;
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("intensity", Intensity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
