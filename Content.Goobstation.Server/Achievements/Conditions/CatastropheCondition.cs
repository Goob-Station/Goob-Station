using Content.Goobstation.Shared.Supermatter.Events;
using Content.Server.Nuke;
using Robust.Server.Player;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class CatastropheCondition : AchievementCondition;

public sealed partial class CatastropheConditionSystem : AchievementConditionSystem<CatastropheCondition>
{
    [Dependency] private IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeExplodedEvent>(OnNukeExploded);
        SubscribeLocalEvent<SupermatterDelaminatedEvent>(OnDelaminated);
    }

    private void OnNukeExploded(NukeExplodedEvent ev)
    {
        if (ev.OwningStation is { } grid)
            ProgressOnGrid(grid);
    }

    private void OnDelaminated(SupermatterDelaminatedEvent ev)
    {
        if (Transform(ev.Supermatter).GridUid is { } grid)
            ProgressOnGrid(grid);
    }

    private void ProgressOnGrid(EntityUid grid)
    {
        foreach (var session in _player.Sessions)
        {
            if (session.AttachedEntity is { } player && Transform(player).GridUid == grid)
                ProgressAll(player);
        }
    }
}
