using Content.Server.Actions;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.Mobs;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Robust.Server.GameObjects;

namespace Content.Server.WhiteDream.BloodCult.Constructs;

public sealed class ConstructSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ConstructComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<ConstructComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ConstructComponent>();
        while (query.MoveNext(out var uid, out var construct))
        {
            if (!construct.Transforming)
                continue;

            construct.TransformAccumulator += frameTime;
            if (construct.TransformAccumulator < construct.TransformDelay)
                continue;

            construct.TransformAccumulator = 0f;
            construct.Transforming = false;
            _appearanceSystem.SetData(uid, ConstructVisualsState.Transforming, false);
        }
    }

    private void OnMapInit(Entity<ConstructComponent> construct, ref MapInitEvent args)
    {
        foreach (var actionId in construct.Comp.Actions)
        {
            var action = _actions.AddAction(construct, actionId);
            construct.Comp.ActionEntities.Add(action);
        }

        _appearanceSystem.SetData(construct, ConstructVisualsState.Transforming, true);
        construct.Comp.Transforming = true;
        var cultistRule = EntityManager.EntityQueryEnumerator<BloodCultRuleComponent>();
        while (cultistRule.MoveNext(out _, out var rule))
            rule.Constructs.Add(construct);
    }

    private void OnComponentShutdown(Entity<ConstructComponent> construct, ref ComponentShutdown args)
    {
        foreach (var actionEntity in construct.Comp.ActionEntities)
            _actions.RemoveAction(actionEntity);

        var cultistRule = EntityManager.EntityQueryEnumerator<BloodCultRuleComponent>();
        while (cultistRule.MoveNext(out _, out var rule))
            rule.Constructs.Remove(construct);
    }

    private void OnMobStateChanged(EntityUid uid, ConstructComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var xform = Transform(uid);
        Spawn(component.SpawnOnDeathPrototype, xform.Coordinates);

        QueueDel(uid);
    }
}
