using Content.Goobstation.Shared.Doodons;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Doodons;

public sealed class DoodonTownHallVisionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float CheckInterval = 1.0f;
    private float _accum;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DoodonTownHallVisionComponent, ToggleTownHallRadiusEvent>(OnToggleRadius);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accum += frameTime;
        if (_accum < CheckInterval)
            return;

        _accum = 0f;

        // Give the toggle action to any CONTROLLED entity with DoodonTownHallVision.
        var query = EntityQueryEnumerator<DoodonTownHallVisionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_mind.TryGetMind(uid, out _, out _))
                continue;

            if (comp.ToggleRadiusActionEntity != null && Exists(comp.ToggleRadiusActionEntity.Value))
                continue;

            _actions.AddAction(uid, ref comp.ToggleRadiusActionEntity, comp.ToggleRadiusAction);
            Dirty(uid, comp);
        }
    }

    private void OnToggleRadius(EntityUid uid, DoodonTownHallVisionComponent comp, ToggleTownHallRadiusEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        comp.ShowTownHallRadius = !comp.ShowTownHallRadius;
        Dirty(uid, comp);

        _popup.PopupEntity(
            comp.ShowTownHallRadius ? "Showing Town Hall influence." : "Hiding Town Hall influence.",
            uid, uid);
    }
}
