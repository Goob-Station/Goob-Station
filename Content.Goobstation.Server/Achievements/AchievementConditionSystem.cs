using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Achievements;

public abstract partial class AchievementConditionSystem<T> : EntitySystem where T : AchievementCondition
{
    [Dependency] protected AchievementSystem Achievements = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    protected readonly List<(AchievementPrototype Proto, T Condition)> Entries = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        Rebuild();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<AchievementPrototype>())
            Rebuild();
    }

    private void Rebuild()
    {
        Entries.Clear();
        foreach (var proto in _proto.EnumeratePrototypes<AchievementPrototype>())
        {
            if (proto.Condition is T condition)
                Entries.Add((proto, condition));
        }
    }

    protected void ProgressAll(EntityUid player, int amount = 1)
    {
        foreach (var (proto, _) in Entries)
        {
            Achievements.AddProgress(player, proto, amount);
        }
    }
}
