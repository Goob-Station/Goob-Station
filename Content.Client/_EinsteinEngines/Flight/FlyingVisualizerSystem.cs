using Content.Client._EinsteinEngines.Flight.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._EinsteinEngines.Flight;

/// <summary>
/// Handles offsetting an entity while flying
/// </summary>
public sealed class FlyingVisualizerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FlightVisualsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlightVisualsComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlightVisualsComponent, BeforePostShaderRenderEvent>(OnBeforeShaderPost);
    }

    private void OnStartup(EntityUid uid, FlightVisualsComponent comp, ComponentStartup args)
    {
        comp.Shader = _protoMan.Index<ShaderPrototype>(comp.AnimationKey).InstanceUnique();
        AddShader(uid, comp.Shader, comp.AnimateLayer, comp.TargetLayer);
        SetValues(comp, comp.Speed, comp.Offset, comp.Multiplier);
    }

    private void OnShutdown(EntityUid uid, FlightVisualsComponent comp, ComponentShutdown args)
    {
        AddShader(uid, null, comp.AnimateLayer, comp.TargetLayer);
    }

    private void AddShader(Entity<SpriteComponent?> entity, ShaderInstance? shader, bool animateLayer, int? layer)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!animateLayer)
            entity.Comp.PostShader = shader;

        if (animateLayer && layer is not null)
            entity.Comp.LayerSetShader(layer.Value, shader);

        entity.Comp.GetScreenTexture = shader is not null;
        entity.Comp.RaiseShaderEvent = shader is not null;
    }

    /// <summary>
    ///     This function can be used to modify the shader's values while its running.
    /// </summary>
    private void OnBeforeShaderPost(EntityUid uid, FlightVisualsComponent comp, ref BeforePostShaderRenderEvent args)
    {
        SetValues(comp, comp.Speed, comp.Offset, comp.Multiplier);
    }

    private void SetValues(FlightVisualsComponent comp, float speed, float offset, float multiplier)
    {
        comp.Shader.SetParameter("Speed", speed);
        comp.Shader.SetParameter("Offset", offset);
        comp.Shader.SetParameter("Multiplier", multiplier);
    }
}
