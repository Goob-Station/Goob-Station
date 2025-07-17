using Content.Shared.GameTicking.Components;
using Content.Server.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Audio;
using Robust.Shared.Random;
using Robust.Shared.Audio.Systems;
using Content.Pirate.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Robust.Server.Player;

namespace Content.Pirate.Server.StationEvents.Events;

internal sealed class PsionicCatGotYourTongueRule : StationEventSystem<PsionicCatGotYourTongueRuleComponent>
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    protected override void Started(EntityUid uid, PsionicCatGotYourTongueRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        List<EntityUid> psionicList = new();

        var query = EntityManager.EntityQueryEnumerator<PsionicComponent, MobStateComponent>();
        while (query.MoveNext(out var psion, out _, out _))
        {
            if (_mobStateSystem.IsAlive(psion) && !HasComp<PsionicInsulationComponent>(psion))
                psionicList.Add(psion);
        }

        foreach (var psion in psionicList)
        {
            var duration = _robustRandom.Next(component.MinDuration, component.MaxDuration);

            _statusEffectsSystem.TryAddStatusEffect(psion,
                "Muted",
                duration,
                false,
                "Muted");

            _sharedAudioSystem.PlayPvs(component.Sound, psion);
        }
    }
}
