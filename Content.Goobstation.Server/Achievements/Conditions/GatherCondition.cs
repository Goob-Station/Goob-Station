using Content.Goobstation.Common.Gatherable;
using Content.Server.Gatherable.Components;
using Content.Shared.Projectiles;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class GatherCondition : AchievementCondition
{
    [DataField]
    public EntityWhitelist? Whitelist;
}

public sealed partial class GatherConditionSystem : AchievementConditionSystem<GatherCondition>
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GatherableComponent, GatheredEvent>(OnGathered);
    }

    private void OnGathered(Entity<GatherableComponent> ent, ref GatheredEvent args)
    {
        if (args.Gatherer is not { } gatherer)
            return;

        if (TryComp<ProjectileComponent>(gatherer, out var projectile) && projectile.Shooter is { } shooter)
            gatherer = shooter;

        foreach (var (proto, condition) in Entries)
        {
            if (_whitelist.IsWhitelistPassOrNull(condition.Whitelist, ent))
                Achievements.AddProgress(gatherer, proto);
        }
    }
}
