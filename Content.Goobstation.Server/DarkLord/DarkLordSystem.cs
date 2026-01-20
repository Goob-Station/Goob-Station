using Content.Goobstation.Shared.DarkLord;
using Robust.Shared.Random;
using Content.Server.GameTicking;

namespace Content.Goobstation.Server.DarkLord;

public sealed class DarkLordSystem : EntitySystem
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DarkLordComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, DarkLordComponent component, MapInitEvent args)
    {
        if (_random.Prob(component.ChosenOneChance))
        {
            var ticker = _entitySystemManager.GetEntitySystem<GameTicker>();
            ticker.AddGameRule("ChosenOneMidround");
            ticker.StartGameRule("ChosenOneMidround");
        }
    }
}
