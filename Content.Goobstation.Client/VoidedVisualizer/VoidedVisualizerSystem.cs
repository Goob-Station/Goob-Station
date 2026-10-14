using Content.Goobstation.Shared.Voidwalker.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.VoidedVisualizer;

public sealed class HologramVisualizerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private readonly ProtoId<ShaderPrototype> _shaderId = "VoidedShader";
    private ShaderPrototype? _shaderProto;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidedVisualsComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<VoidedVisualsComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentInit(EntityUid uid, VoidedVisualsComponent component, ComponentInit args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.PostShader = (_shaderProto ??= _protoMan.Index(_shaderId)).InstanceUnique();
    }

    private void OnComponentShutdown(EntityUid uid, VoidedVisualsComponent component, ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.PostShader = null;
    }
}
