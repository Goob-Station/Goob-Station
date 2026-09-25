using Content.Client._Shitcode.Heretic;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Makes entities invisible in darkness and visible in light, and hides their status icons from everyone else.
/// </summary>
public sealed class BoogeymanShadowSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherBoogeyman";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BoogeymanShadowComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BoogeymanShadowComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<GetStatusIconsEvent>(OnGetStatusIcons, after: [typeof(GhoulSystem)]);
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

    private void OnGetStatusIcons(ref GetStatusIconsEvent args)
    {
        if (args.Uid != _player.LocalEntity && HasComp<BoogeymanShadowComponent>(args.Uid))
            args.StatusIcons.Clear();
    }
}
