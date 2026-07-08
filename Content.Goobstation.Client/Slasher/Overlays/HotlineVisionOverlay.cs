using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Overlay for the chicken mask slasher.
/// </summary>
public sealed class HotlineVisionOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherHotline";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace | OverlaySpace.WorldSpaceBelowWorld;

    private readonly ShaderInstance _voidShader;
    private readonly ShaderInstance _spaceShader;

    public HotlineVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        ZIndex = 1;

        var phase = Random.Shared.NextSingle() * 1000f;

        _voidShader = _proto.Index(Shader).InstanceUnique();
        _voidShader.SetParameter("phase", phase);

        _voidShader.Stencil = new StencilParameters
        {
            Enabled = true,
            Ref = 1,
            Func = StencilFunc.Equal,
            Op = StencilOp.Keep,
            WriteMask = 0,
        };

        _spaceShader = _proto.Index(Shader).InstanceUnique();
        _spaceShader.SetParameter("phase", phase);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return _entMan.HasComponent<HotlineVisionComponent>(_player.LocalEntity)
               && _entMan.TryGetComponent(_player.LocalEntity, out EyeComponent? eye)
               && args.Viewport.Eye == eye.Eye;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var shader = args.Space == OverlaySpace.WorldSpaceBelowWorld ? _spaceShader : _voidShader;

        var handle = args.WorldHandle;
        handle.UseShader(shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
