using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Wraith.Insanity;

public sealed class InsanityOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "Insanity";

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _insanityShader;

    private float _speed = 1f;
    private float _radius = 1f;
    private Color _effectColor = Color.Red;

    public InsanityOverlay()
    {
        IoCManager.InjectDependencies(this);
        _insanityShader = _prototypeManager.Index(Shader).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        return true;
    }

    #region Public
    public void SetValues(float speed, float radius, Color color)
    {
        _speed = speed;
        _radius = radius;
        _effectColor = color;
    }
    #endregion

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _insanityShader.SetParameter("effect_color", new Vector4(_effectColor.R, _effectColor.G, _effectColor.B, _effectColor.A));
        _insanityShader.SetParameter("radius", _radius);
        _insanityShader.SetParameter("speed", _speed);
        _insanityShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        handle.UseShader(_insanityShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
