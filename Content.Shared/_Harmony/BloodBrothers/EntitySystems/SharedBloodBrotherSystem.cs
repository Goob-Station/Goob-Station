using Content.Shared._Harmony.BloodBrothers.Components;
using Content.Shared.Actions;
using Content.Shared.Antag;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared._Harmony.BloodBrothers.EntitySystems;

public abstract class SharedBloodBrotherSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InitialBloodBrotherComponent, MapInitEvent>(OnInitialBloodBrotherMapInit);
        SubscribeLocalEvent<InitialBloodBrotherComponent, ComponentShutdown>(OnInitialBloodBrotherShutdown);
        SubscribeLocalEvent<BloodBrotherComponent, ComponentGetStateAttemptEvent>(OnBloodBrotherAttemptGetState);
    }

    private void OnInitialBloodBrotherMapInit(Entity<InitialBloodBrotherComponent> entity, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(entity, ref entity.Comp.ConvertActionEntity, entity.Comp.ConvertAction);
        _actionsSystem.AddAction(entity, ref entity.Comp.CheckConvertActionEntity, entity.Comp.CheckConvertAction);
        Dirty(entity);
    }

    private void OnInitialBloodBrotherShutdown(Entity<InitialBloodBrotherComponent> entity, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(entity.Comp.ConvertActionEntity);
        _actionsSystem.RemoveAction(entity.Comp.CheckConvertActionEntity);
    }

    private void OnBloodBrotherAttemptGetState(
        Entity<BloodBrotherComponent> entity,
        ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not {} uid)
            return true;

        return HasComp<BloodBrotherComponent>(uid) || HasComp<ShowAntagIconsComponent>(uid);
    }
}
