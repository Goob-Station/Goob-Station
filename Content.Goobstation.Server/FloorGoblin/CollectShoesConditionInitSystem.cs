using Content.Goobstation.Shared.FloorGoblin;
using Robust.Server.Player;
using Robust.Shared.Enums;

namespace Content.Goobstation.Server.FloorGoblin;

public sealed class CollectShoesConditionInitSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CollectShoesConditionComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, CollectShoesConditionComponent comp, ComponentInit args)
    {
        if (comp.Required > 0)
            return;

        var count = 0;
        foreach (var s in _players.Sessions)
            if (s.Status == SessionStatus.InGame)
                count++;

        var target = (int) Math.Ceiling(comp.Base + comp.PerPlayer * count);
        if (target < comp.Min) target = comp.Min;
        if (target > comp.Max) target = comp.Max;
        comp.Required = target;
    }
}
