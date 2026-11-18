using Content.Client.Graphics;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Makes entities invisible in darkness and visible in light.
/// </summary>
public sealed partial class BoogeymanShadowSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherBoogeyman";

    private const string PostShaderId = "slasher-boogeyman";

    private static readonly string[] RunAfter =
    [
        ContentPostShaderIds.Stealth,
        ContentPostShaderIds.FloorOcclusion,
        ContentPostShaderIds.Holopad,
        ContentPostShaderIds.InteractionOutline,
        ContentPostShaderIds.TargetOutline,
        ContentPostShaderIds.DragDropOutline,
    ];

    [SubscribeLocalEvent]
    private void OnStartup(Entity<BoogeymanShadowComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            ApplyShader((ent.Owner, sprite));
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<BoogeymanShadowComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            _sprite.RemovePostShader((ent.Owner, sprite), PostShaderId);
    }

    private void ApplyShader(Entity<SpriteComponent> sprite)
    {
        _sprite.SetPostShader((sprite.Owner, sprite.Comp),
            new SpriteComponent.PostShaderArgs(PostShaderId, _proto.Index(Shader).InstanceUnique())
            {
                After = RunAfter,
            });
    }
}
