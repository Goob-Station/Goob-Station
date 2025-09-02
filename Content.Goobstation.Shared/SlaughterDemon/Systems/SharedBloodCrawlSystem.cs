using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids.Components;
using Content.Shared.Polymorph;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SlaughterDemon.Systems;


/// <summary>
/// This handles the blood crawl system.
/// Blood Crawl allows you to jaunt, as long as you activate it in a pool of blood.
/// To exit the jaunt, you must also stand on a poll of blood.
/// </summary>
public abstract class SharedBloodCrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<ActionsComponent> _actionQuery;
    private EntityQuery<PuddleComponent> _puddleQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _actionQuery = GetEntityQuery<ActionsComponent>();
        _puddleQuery = GetEntityQuery<PuddleComponent>();

        SubscribeLocalEvent<BloodCrawlComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<BloodCrawlComponent, BloodCrawlEvent>(OnBloodCrawl);
    }

    private void OnStartup(EntityUid uid, BloodCrawlComponent component, ComponentStartup args)
    {
        if (!_actionQuery.TryGetComponent(uid, out var actions))
            return;

        _actions.AddAction(uid, component.ActionId, component: actions);
    }

    private void OnBloodCrawl(EntityUid uid, BloodCrawlComponent component, BloodCrawlEvent args)
    {
        if (!IsStandingOnBlood(uid))
        {
            _actions.SetCooldown(args.Action.Owner, component.ActionCooldown);
            return;
        }

        component.IsCrawling = !component.IsCrawling;

        if (!CheckAlreadyCrawling((uid, component)))
            return;

        var evAttempt = new BloodCrawlAttemptEvent();
        RaiseLocalEvent(uid, ref evAttempt);

        _audio.PlayPvs(component.EnterJauntSound, Transform(uid).Coordinates);

        PolymorphDemon(uid, component.Jaunt);

        args.Handled = true;
    }

    #region Helper Functions

    /// <summary>
    /// Detects if an entity is standing on blood, or not.
    /// </summary>
    public bool IsStandingOnBlood(Entity<BloodCrawlComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        var ents = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange);
        foreach (var entity in ents)
        {
            if (!_puddleQuery.TryComp(entity, out var puddle))
                continue;

            if (!_solutionContainerSystem.ResolveSolution(entity, puddle.SolutionName, ref puddle.Solution, out var solution))
                continue;

            foreach (var reagent in solution.Contents)
            {
                if (reagent.Reagent.Prototype == ent.Comp.Blood)
                    return true;
            }
        }
        return false;
    }

    protected virtual bool CheckAlreadyCrawling(Entity<BloodCrawlComponent> ent)
    {
        return false;
    }

    protected virtual void PolymorphDemon(EntityUid user, ProtoId<PolymorphPrototype> polymorph) {}

    #endregion
}
