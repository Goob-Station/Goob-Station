using Content.Client.Crayon;
using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared._CorvaxGoob.CustomShader;
using Content.Shared.Crayon;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Client._CorvaxGoob.CustomShader;

public sealed class CustomShaderSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomShaderComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CustomShaderComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<CustomShaderComponent, AfterAutoHandleStateEvent>(OnHandleState);

        _sawmill = _logManager.GetSawmill("customshader");
    }

    private void OnHandleState(Entity<CustomShaderComponent> entity, ref AfterAutoHandleStateEvent args)
    {
        if (entity.Comp.Shader is null)
            return;

        ApplyShader(entity, entity.Comp.Shader);
    }

    private void OnComponentInit(Entity<CustomShaderComponent> entity, ref ComponentInit ev)
    {
        if (entity.Comp.Shader is null)
            return;

        ApplyShader(entity, entity.Comp.Shader);
    }

    private void OnComponentShutdown(Entity<CustomShaderComponent> entity, ref ComponentShutdown ev)
    {
        if (entity.Comp.Shader is null)
            return;

        ClearShader(entity);
    }

    private void ApplyShader(EntityUid entity, string shaderId)
    {
        if (!_prototype.TryIndex<ShaderPrototype>(shaderId, out var shaderPrototype))
        {
            _sawmill.Error("Invalid custom shader prototype id " + shaderId);
            return;
        }

        if (TryComp<SpriteComponent>(entity, out var sprite))
            sprite.PostShader = shaderPrototype.InstanceUnique();
    }

    private void ClearShader(EntityUid entity)
    {
        if (TryComp<SpriteComponent>(entity, out var sprite))
            sprite.PostShader = null;
    }
}
