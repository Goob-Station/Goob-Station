using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids.Components;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SlaughterDemon.Systems;


/// <summary>
/// This handles the reagent crawling system.
/// Reagent Crawling allows you to jaunt, as long as you activate it in a pool of the target reagent.
/// To exit the jaunt, you must also stand on the reagent.
/// </summary>
public abstract class SharedReagentCrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<ActionsComponent> _actionQuery;
    private EntityQuery<PuddleComponent> _puddleQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _actionQuery = GetEntityQuery<ActionsComponent>();
        _puddleQuery = GetEntityQuery<PuddleComponent>();

        SubscribeLocalEvent<ReagentCrawlComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<ReagentCrawlComponent, ReagentCrawlEvent>(OnBloodCrawl);
    }

    private void OnStartup(EntityUid uid, ReagentCrawlComponent  component, ComponentStartup args)
    {
        if (!_actionQuery.TryGetComponent(uid, out var actions))
            return;

        _actions.AddAction(uid, component.ActionId, component: actions);
    }

    private void OnReagentCrawl(EntityUid uid, ReagentCrawlComponent  component, ReagentCrawlComponent  args)
    {
        if (!IsStandingOnTargetReagent((uid, component)))
        {
            _popup.PopupPredicted(Loc.GetString(component.EnterJauntFailMessage), uid, uid);
            _actions.SetCooldown(args.Action.Owner, component.ActionCooldown);
            return;
        }

        component.IsCrawling = !component.IsCrawling;
        Dirty(uid, component);

        if (!CheckAlreadyCrawling((uid, component)))
            return;

        var evAttempt = new ReagentCrawlAttemptEvent();
        RaiseLocalEvent(uid, ref evAttempt);

        if (evAttempt.Cancelled)
            return;

        _audio.PlayPredicted(component.EnterJauntSound, Transform(uid).Coordinates, uid);

        PolymorphEntity(uid, component.Jaunt);

        args.Handled = true;
    }

    #region Helper Functions

    /// <summary>
    /// Detects if an entity is standing on blood, or not.
    /// </summary>
    public bool IsStandingOnTargetReagent(Entity<ReagentCrawlComponent> ent)
    {
        var ents = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange);
        foreach (var entity in ents)
        {
            if (!_puddleQuery.TryComp(entity, out var puddle))
                continue;

            if (!_solutionContainerSystem.ResolveSolution(entity, puddle.SolutionName, ref puddle.Solution, out var solution))
                continue;

            foreach (var reagent in solution.Contents)
            {
                if (ent.Comp.TargetReagent.Contains(reagent.Reagent.Prototype)
                    && reagent.Quantity >= ent.Comp.RequiredReagentAmount)
                    return true;
            }
        }
        return false;
    }

    protected virtual bool CheckAlreadyCrawling(Entity<ReagentCrawlComponent> ent)
    {
        return false;
    }

    protected virtual void PolymorphEntity(EntityUid user, ProtoId<PolymorphPrototype> polymorph) {}

    #endregion
}
