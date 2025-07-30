using Robust.Shared.Random;
using Content.Server.Abilities.Psionics;
using Content.Shared.GameTicking.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Server.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Mobs.Systems;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Zombies;
using Content.Pirate.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;

namespace Content.Pirate.Server.StationEvents.Events;

internal sealed class NoosphericStormRule : StationEventSystem<NoosphericStormRuleComponent>
{
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    protected override void Started(EntityUid uid, NoosphericStormRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        List<EntityUid> validList = new();

        var query = EntityManager.EntityQueryEnumerator<PsionicComponent>();
        while (query.MoveNext(out var Psionic, out var PsionicComponent))
        {
            if (_mobStateSystem.IsDead(Psionic)
                || HasComp<PsionicInsulationComponent>(Psionic))
                continue;

            validList.Add(Psionic);
        }

        // Give some targets psionic abilities.
        RobustRandom.Shuffle(validList);

        var toAwaken = RobustRandom.Next(1, component.MaxAwaken);

        foreach (var target in validList)
        {
            if (toAwaken-- == 0)
                break;

            _psionicAbilitiesSystem.AddPsionics(target);
        }

        // Increase glimmer.
        var baseGlimmerAdd = _robustRandom.Next(component.BaseGlimmerAddMin, component.BaseGlimmerAddMax);
        var glimmerAdded = baseGlimmerAdd;

        _glimmerSystem.DeltaGlimmerInput(glimmerAdded);
    }
}
