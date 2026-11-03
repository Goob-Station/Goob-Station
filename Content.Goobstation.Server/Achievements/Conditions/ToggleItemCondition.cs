using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class ToggleItemCondition : AchievementCondition
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();
}

public sealed partial class ToggleItemConditionSystem : AchievementConditionSystem<ToggleItemCondition>
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemToggleComponent, ItemToggledEvent>(OnToggled);
    }

    private void OnToggled(Entity<ItemToggleComponent> ent, ref ItemToggledEvent args)
    {
        if (!args.Activated || args.User is not { } user)
            return;

        foreach (var (proto, condition) in Entries)
        {
            if (_whitelist.IsWhitelistPass(condition.Whitelist, ent))
                Achievements.AddProgress(user, proto);
        }
    }
}
