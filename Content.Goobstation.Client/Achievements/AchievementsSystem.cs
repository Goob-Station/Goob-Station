using Content.Goobstation.Shared.Achievements;

namespace Content.Goobstation.Client.Achievements;

public sealed class AchievementsSystem : EntitySystem
{
    public List<AchievementData>? Achievements { get; private set; }

    public event Action? Updated;
    public event Action<AchievementData>? Unlocked;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<AchievementsStateEvent>(OnState);
        SubscribeNetworkEvent<AchievementUnlockedEvent>(OnUnlocked);
    }

    public void RequestState()
    {
        RaiseNetworkEvent(new RequestAchievementsEvent());
    }

    private void OnState(AchievementsStateEvent ev)
    {
        Achievements = ev.Achievements;
        Updated?.Invoke();
    }

    private void OnUnlocked(AchievementUnlockedEvent ev)
    {
        if (Achievements != null)
        {
            var index = Achievements.FindIndex(a => a.Id == ev.Achievement.Id);
            if (index >= 0)
                Achievements[index] = ev.Achievement;
            else
                Achievements.Add(ev.Achievement);
        }

        Unlocked?.Invoke(ev.Achievement);
        Updated?.Invoke();
    }
}
