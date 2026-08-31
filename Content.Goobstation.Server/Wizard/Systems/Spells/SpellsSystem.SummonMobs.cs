using Content.Goobstation.Shared.Wizard.Events;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class SpellsSystem
{
    protected override void SummonMobsRelay(SummonMobsEvent ev)
    {
        if (ev.Mobs.Count == 0)
            return;

        var positions =
            GetSpawnCoordinatesAroundPerformer(ev.Performer, ev.Range, ev.Amount, ev.SpawnAngle, ev.CollisionMask);
        foreach (var pos in positions)
        {
            var mob = Spawn(_random.Pick(ev.Mobs), pos);

            if (ev.FactionIgnoreSummoner)
                _faction.IgnoreEntity(mob, ev.Performer);
        }
    }
}