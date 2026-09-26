using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Overlays;

public sealed class SlasherValentineOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> ValentineShader = "SlasherFearValentine";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;

    public float Intensity;

    public SlasherValentineOverlay()
    {
        IoCManager.InjectDependencies(this);

        _shader = _proto.Index(ValentineShader).InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null
            || Intensity <= 0.001f
            || !_entMan.HasComponent<SlasherValentineOverlayComponent>(_player.LocalEntity)
            || !_entMan.TryGetComponent(_player.LocalEntity, out EyeComponent? eye)
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
