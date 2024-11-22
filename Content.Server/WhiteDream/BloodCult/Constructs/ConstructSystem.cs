using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Robust.Server.GameObjects;

namespace Content.Server.WhiteDream.BloodCult.Constructs;

public sealed class ConstructSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ConstructComponent, ComponentShutdown>(OnComponentShutdown);
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

    private void OnComponentStartup(Entity<ConstructComponent> ent, ref ComponentStartup args)
    {
        _appearanceSystem.SetData(ent, ConstructVisualsState.Transforming, true);
        ent.Comp.Transforming = true;
        var cultistRule = EntityManager.EntityQueryEnumerator<BloodCultRuleComponent>();
        while (cultistRule.MoveNext(out _, out var rule))
        {
            rule.Constructs.Add(ent);
        }
    }

    private void OnComponentShutdown(Entity<ConstructComponent> ent, ref ComponentShutdown args)
    {
        var cultistRule = EntityManager.EntityQueryEnumerator<BloodCultRuleComponent>();
        while (cultistRule.MoveNext(out _, out var rule))
        {
            rule.Constructs.Remove(ent);
        }
    }
}
