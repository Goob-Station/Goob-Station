using Content.Shared.GameTicking.Components;
using Content.Server.Abilities.Psionics;
using Content.Server.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Mobs.Systems;
using Content.Pirate.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Robust.Shared.Random;
using Content.Shared.Damage.Systems;

namespace Content.Pirate.Server.StationEvents.Events;

internal sealed class NoosphericZapRule : StationEventSystem<NoosphericZapRuleComponent>
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    protected override void Started(EntityUid uid, NoosphericZapRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        List<EntityUid> psionicList = new();

        var query = EntityManager.EntityQueryEnumerator<PsionicComponent>();
        while (query.MoveNext(out var psion, out _))
        {
            if (_mobStateSystem.IsAlive(psion) && !HasComp<PsionicInsulationComponent>(psion))
                psionicList.Add(psion);
        }

        foreach (var psion in psionicList)
        {
            // Stun the psionic
            _stamina.TakeStaminaDamage(psion, (float) 100);

            // Potentially modify power reroll chances if they have rerolls available
            if (TryComp<PsionicComponent>(psion, out var psionicComp) && psionicComp.CanReroll)
            {
                // This is a simplified version - you might want to implement more complex logic
                if (_random.Prob(component.PowerRerollMultiplier))
                {
                    _psionicAbilitiesSystem.AddPsionics(psion);
                }
            }
        }
    }
}
