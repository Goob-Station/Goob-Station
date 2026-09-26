using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Makes entities invisible in darkness and visible in light.
/// </summary>
public sealed class BoogeymanShadowSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherBoogeyman";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BoogeymanShadowComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BoogeymanShadowComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<BoogeymanShadowComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.PostShader = _proto.Index(Shader).InstanceUnique();
    }

    private void OnShutdown(Entity<BoogeymanShadowComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.PostShader = null;
    }
}
