using Content.Shared._ES.Viewcone;
using Content.Shared.MouseRotator;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._ES.Viewcone.Overlays;

public sealed class ESViewconeConeOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private SharedTransformSystem _transform = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _viewconeShader;

    private float _coneAngle;
    private float _coneFeather;
    private float _coneIgnoreRadius;
    private float _coneIgnoreFeather;

    private float _viewAngle;

    public ESViewconeConeOverlay()
    {
        IoCManager.InjectDependencies(this);
        _transform = _entityManager.System<SharedTransformSystem>();
        _viewconeShader = _prototypeManager.Index<ShaderPrototype>("Viewcone").InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalSession?.AttachedEntity, out ESViewconeComponent? viewComp))
            return false;

        _coneAngle = viewComp.ConeAngle;
        _coneFeather = viewComp.ConeFeather;
        _coneIgnoreRadius = (viewComp.ConeIgnoreRadius - viewComp.ConeIgnoreFeather) * 50f;
        _coneIgnoreFeather = Math.Max(viewComp.ConeIgnoreFeather * 200f, 8f);

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var playerEntity = _playerManager.LocalSession?.AttachedEntity;

        if (playerEntity == null)
            return;


        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;

        var zoom = 1.0f;
        var eyeAngle = 0.0f;
        var playerAngle = (float) _transform.GetWorldRotation(playerEntity.Value).Theta;
        if (_entityManager.TryGetComponent<EyeComponent>(playerEntity, out var eyeComponent))
        {
            zoom = eyeComponent.Zoom.X;
            eyeAngle = (float) eyeComponent.Rotation.Theta;
        }


        if (_entityManager.TryGetComponent<MouseRotatorComponent>(playerEntity, out var mouse))
        {
            var mousePos = _eyeManager.PixelToMap(_inputManager.MouseScreenPosition);
            if (mousePos.MapId != MapId.Nullspace)
                playerAngle = (float) (mousePos.Position - _transform.GetMapCoordinates(playerEntity.Value).Position).ToAngle().Theta + MathHelper.DegreesToRadians(90f);
        }

        _viewAngle = playerAngle + eyeAngle;

        _viewconeShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        _viewconeShader.SetParameter("Zoom", zoom);

        _viewconeShader.SetParameter("ViewAngle", _viewAngle);

        _viewconeShader.SetParameter("ConeAngle", _coneAngle);
        _viewconeShader.SetParameter("ConeFeather", _coneFeather);
        _viewconeShader.SetParameter("ConeIgnoreRadius", _coneIgnoreRadius);
        _viewconeShader.SetParameter("ConeIgnoreFeather", _coneIgnoreFeather);

        worldHandle.UseShader(_viewconeShader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
    }
}
