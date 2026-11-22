using Content.Shared._Lavaland.Aggression;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class BossKillCondition : AchievementCondition
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public List<EntProtoId>? Prototypes;
}

public sealed partial class BossKillConditionSystem : AchievementConditionSystem<BossKillCondition>
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AggressiveComponent, AggressiveKilledEvent>(OnKilled);
    }

    private void OnKilled(Entity<AggressiveComponent> ent, ref AggressiveKilledEvent args)
    {
        foreach (var (proto, condition) in Entries)
        {
            if (!_whitelist.IsWhitelistPassOrNull(condition.Whitelist, ent)
                || condition.Prototypes is { } prototypes
                && (Prototype(ent) is not { } entProto || !prototypes.Contains(entProto.ID)))
                continue;

            foreach (var aggressor in args.Aggressors)
            {
                Achievements.AddProgress(aggressor, proto);
            }
        }
    }
}
