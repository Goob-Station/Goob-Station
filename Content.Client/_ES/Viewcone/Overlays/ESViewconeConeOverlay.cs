using Content.Shared._ES.Viewcone;
using Content.Shared.MouseRotator;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._ES.Viewcone.Overlays;

/// <summary>
///     Renders the actual "cone" part of the viewcone, no alpha modulation
/// </summary>
public sealed class ESViewconeConeOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private readonly SharedTransformSystem _xform;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public static ProtoId<ShaderPrototype> ShaderPrototype = "Viewcone";
    private readonly ShaderInstance _viewconeShader;

    private Entity<EyeComponent, TransformComponent>? _eyeEntity;
    private float _coneAngle;
    private float _coneFeather;
    private float _coneIgnoreRadius;
    private float _coneIgnoreFeather;

    public ESViewconeConeOverlay()
    {
        IoCManager.InjectDependencies(this);
        _xform = _ent.System<SharedTransformSystem>();
        _viewconeShader = _proto.Index(ShaderPrototype).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        _eyeEntity = null;

        // This is really stupid but there isn't another way to reverse an eye entity from just an IEye afaict
        // It's not really inefficient though. theres barely any of those fuckin things anyway (? verify that) (maybe this scales with players in view) (shit)
        var enumerator = _ent.AllEntityQueryEnumerator<EyeComponent, ESViewconeComponent, TransformComponent>();
        while (enumerator.MoveNext(out var uid, out var eye, out var viewcone, out var xform))
        {
            if (args.Viewport.Eye != eye.Eye)
                continue;

            _coneAngle = viewcone.ConeAngle;
            _coneFeather = viewcone.ConeFeather;
            _coneIgnoreRadius = (viewcone.ConeIgnoreRadius - viewcone.ConeIgnoreFeather) * 50f;
            _coneIgnoreFeather = Math.Max(viewcone.ConeIgnoreFeather * 200f, 8f);
            _eyeEntity = (uid, eye, xform);
            break;
        }

        return _eyeEntity != null;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null || _eyeEntity == null)
            return;

        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;

        var zoom = _eyeEntity.Value.Comp1.Zoom.X;
        var eyeAngle = (float) _eyeEntity.Value.Comp1.Rotation.Theta;
        var playerAngle = (float) _xform.GetWorldRotation(_eyeEntity.Value.Comp2).Theta;

        if (_ent.HasComponent<MouseRotatorComponent>(_eyeEntity))
        {
            var mousePos = _eye.PixelToMap(_input.MouseScreenPosition);
            if (mousePos.MapId != MapId.Nullspace)
                playerAngle = (float) (mousePos.Position - _xform.GetMapCoordinates(_eyeEntity.Value).Position).ToAngle().Theta + MathHelper.DegreesToRadians(90f);
        }

        var viewAngle = playerAngle + eyeAngle;

        _viewconeShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _viewconeShader.SetParameter("Zoom", zoom);
        _viewconeShader.SetParameter("ViewAngle", viewAngle);
        _viewconeShader.SetParameter("ConeAngle", _coneAngle);
        _viewconeShader.SetParameter("ConeFeather", _coneFeather);
        _viewconeShader.SetParameter("ConeIgnoreRadius", _coneIgnoreRadius);
        _viewconeShader.SetParameter("ConeIgnoreFeather", _coneIgnoreFeather);

        worldHandle.UseShader(_viewconeShader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
        _eyeEntity = null;
    }
}
