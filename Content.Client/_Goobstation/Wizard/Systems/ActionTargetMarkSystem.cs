using Content.Shared.GameTicking;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Wizard.Systems;

public sealed class ActionTargetMarkSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;

    private static readonly EntProtoId MarkProto = "ActionTargetMark";

    public EntityUid? Target;
    public EntityUid? Mark;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        Target = null;
        Mark = null;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        Target = null;
        Mark = null;
    }

    public void SetMark(EntityUid? uid)
    {
        if (Target == uid)
            return;
        Target = uid;
        if (uid == null)
        {
            QueueDel(Mark);
            Mark = null;
            return;
        }

        if (!TryComp(uid, out TransformComponent? xform))
            return;
        Mark ??= SpawnAttachedTo(MarkProto, xform.Coordinates);
        var markXform = EnsureComp<TransformComponent>(Mark.Value);
        _transform.SetCoordinates(Mark.Value, markXform, xform.Coordinates);
        _transform.SetParent(Mark.Value, markXform, uid.Value, xform);
    }
}
