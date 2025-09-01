using Content.Server.EUI;
using Content.Shared.Objectives.Components;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Server.Objectives.Systems;

public sealed class BlankObjectiveSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlankObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<BlankObjectiveComponent, ObjectiveAssignedEvent>(OnPersonAssigned);
    }

    private void OnGetProgress(EntityUid uid, BlankObjectiveComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 1;
    }

    private void OnPersonAssigned(EntityUid uid, BlankObjectiveComponent comp, ref ObjectiveAssignedEvent args)
    {
        if (args.Mind.OwnedEntity is not null
        && comp.SelfDefined
        && _playerManager.TryGetSessionByEntity((EntityUid)args.Mind.OwnedEntity, out var session))
            RunEui(uid, session);
    }

    public void RunEui(EntityUid uid, ICommonSession? session)
    {
        if (session is null)
            return;

        var ui = new BlankObjectiveEui(_metaData, uid);
        _euiManager.OpenEui(ui, session);
    }

}
