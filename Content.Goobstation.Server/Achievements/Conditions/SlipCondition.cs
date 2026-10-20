using Content.Shared.Slippery;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class SlipCondition : AchievementCondition
{
    [DataField]
    public EntityWhitelist? Whitelist;
}

public sealed partial class SlipConditionSystem : AchievementConditionSystem<SlipCondition>
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlipperyComponent, SlipEvent>(OnSlip);
    }

    private void OnSlip(Entity<SlipperyComponent> ent, ref SlipEvent args)
    {
        foreach (var (proto, condition) in Entries)
        {
            if (_whitelist.IsWhitelistPassOrNull(condition.Whitelist, ent))
                Achievements.AddProgress(args.Slipped, proto);
        }
    }
}
