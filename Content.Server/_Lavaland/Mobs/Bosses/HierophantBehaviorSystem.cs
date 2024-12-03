using Content.Server.Audio;
using Content.Server.Destructible;
using Content.Shared._Lavaland.Mobs;
using Content.Shared._Lavaland.Mobs.Bosses;
using Content.Shared.Audio;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;

namespace Content.Server._Lavaland.Mobs.Bosses;

public sealed partial class HierophantBehaviorSystem : EntitySystem
{
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly MegafaunaSystem _megafauna = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HierophantBossComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (TryComp<AggressorsComponent>(uid, out var aggressors))
            {
                if (aggressors.Aggressors.Count > 0 && !comp.Aggressive)
                    InitBoss((uid, comp));
                else continue;
            }

            if (comp.Aggressive)
            {
                // todo tick all timers
            }
        }
    }

    #region Boss specific stuff

    private void InitBoss(Entity<HierophantBossComponent> ent)
    {
        ent.Comp.Aggressive = true;

        // add ambient
        if (TryComp<AmbientSoundComponent>(ent, out var ambient))
            _ambient.SetAmbience(ent, true, ambient);
    }

    // todo add attacks
    private void PickRandomTarget(Entity<HierophantBossComponent> ent)
    {

    }

    private void DoMove(Entity<HierophantBossComponent> ent)
    {

    }
    private void DoMinorAttack(EntityUid target)
    {

    }
    private void DoMajorAttack(EntityUid target)
    {

    }
    private void SpawnChaser(EntityUid target)
    {

    }

    #endregion
}
