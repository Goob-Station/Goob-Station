using Content.Goobstation.Common.Tools;
using Content.Shared.Tools.Systems;

namespace Content.Goobstation.Server.Tools;

public sealed class WeldingSparksSystem : EntitySystem
{
    private EntityUid? _sparksEffect;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeldingSparksComponent, UseToolEvent>(OnUseTool);
        SubscribeLocalEvent<WeldingSparksComponent, SharedToolSystem.ToolDoAfterEvent>(OnAfterUseTool);
    }

    private void OnUseTool(Entity<WeldingSparksComponent> ent, ref UseToolEvent args)
    {
        if (args.Target is not { } target)
            return;

        _sparksEffect = Spawn(ent.Comp.Effect, Transform(target).Coordinates);
    }

    private void OnAfterUseTool(Entity<WeldingSparksComponent> ent, ref SharedToolSystem.ToolDoAfterEvent args)
    {
        QueueDel(_sparksEffect);
    }
}
