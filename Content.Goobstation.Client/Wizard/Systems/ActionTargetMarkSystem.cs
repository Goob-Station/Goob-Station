using Content.Goobstation.Shared.ActionTargetMarkSystem;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Wizard.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class ActionTargetMarkSystem : SharedActionTargetMarkSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly EntProtoId MarkProto = "ActionTargetMark";

    public override void SetMark(EntityUid user, EntityUid? target, ActionTargetMarkComponent? actionTargetMarkComponent = null)
    {
        if(!Resolve(user, ref actionTargetMarkComponent) || actionTargetMarkComponent.Target == target)
            return;

        actionTargetMarkComponent.Target = target;
        if (actionTargetMarkComponent.Target == null)
        {
            QueueDel(actionTargetMarkComponent.Mark);
            actionTargetMarkComponent.Mark = null;
            return;
        }

        if (!TryComp(target, out TransformComponent? xform))
            return;

        actionTargetMarkComponent.Mark ??= SpawnAttachedTo(MarkProto, xform.Coordinates);
        var markXform = EnsureComp<TransformComponent>(actionTargetMarkComponent.Mark.Value);
        _transform.SetCoordinates(actionTargetMarkComponent.Mark.Value, markXform, xform.Coordinates);
        _transform.SetParent(actionTargetMarkComponent.Mark.Value, markXform, target.Value, xform);
    }
}
