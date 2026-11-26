using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;

namespace Content.Goobstation.Server.Bongle;

public sealed partial class ChainRulesRuleSystem : GameRuleSystem<ChainRulesRuleComponent>
{
    [Dependency] private GameTicker _gameTicker = default!;

    protected override void Started(EntityUid uid, ChainRulesRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        foreach (var rule in component.Rules)
        {
            _gameTicker.StartGameRule(rule, out _);
        }
    }
}
