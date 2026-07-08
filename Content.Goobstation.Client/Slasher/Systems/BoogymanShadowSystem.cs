using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Makes entities invisible in darkness and visible in light.
/// </summary>
public sealed class BoogymanShadowSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherBoogyman";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BoogymanShadowComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BoogymanShadowComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<BoogymanShadowComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.PostShader = _proto.Index(Shader).InstanceUnique();
    }

    private void OnShutdown(Entity<BoogymanShadowComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.PostShader = null;
    }
}
