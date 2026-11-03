using Content.Goobstation.Shared.Vehicles;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class VehicleHornCondition : AchievementCondition
{
    [DataField]
    public EntityWhitelist? Whitelist;
}

public sealed partial class VehicleHornConditionSystem : AchievementConditionSystem<VehicleHornCondition>
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VehicleComponent, VehicleHornedEvent>(OnHorned);
    }

    private void OnHorned(Entity<VehicleComponent> ent, ref VehicleHornedEvent args)
    {
        foreach (var (proto, condition) in Entries)
        {
            if (_whitelist.IsWhitelistPassOrNull(condition.Whitelist, ent))
                Achievements.AddProgress(args.Driver, proto);
        }
    }
}
