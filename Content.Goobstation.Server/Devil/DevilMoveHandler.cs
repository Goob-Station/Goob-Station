using Content.Goobstation.Common.Devil;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.CrematorImmune;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Supermatter.Components;
using Content.Server.Antag.Components;
using Content.Server.Atmos.Components;
using Content.Server.Speech.Components;
using Content.Server.Zombies;
using Content.Shared._Lavaland.Chasm;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Shuttles.Components;

namespace Content.Goobstation.Server.Devil;

public sealed class DevilMoveHandler : MoveDevilCompsComm
{
    [Dependency] private readonly MoveDevilCompsComm _moveDevil = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
    private static readonly Type[] DevilCompsToRemove =
    {
        // Taken from devilsystem
        typeof(DevilComponent),
        typeof(ZombieImmuneComponent),
        typeof(BreathingImmunityComponent),
        typeof(PressureImmunityComponent),
        typeof(ActiveListenerComponent),
        typeof(WeakToHolyComponent),
        typeof(CrematoriumImmuneComponent),
        typeof(AntagImmuneComponent),
        typeof(SupermatterImmuneComponent),
        typeof(PreventChasmFallingComponent),
        typeof(FTLSmashImmuneComponent),
        typeof(CheatDeathComponent),
    };

    public override void MoveDevilComps(EntityUid oldBody, EntityUid uid)
    {


        foreach (var comp in DevilCompsToRemove)
        {
            if (!EntityManager.TryGetComponent(oldBody, comp, out var sourceComp))
                continue;
            CopyComp(oldBody, uid, sourceComp);
            RemComp(oldBody, comp);
        }
    }
}
