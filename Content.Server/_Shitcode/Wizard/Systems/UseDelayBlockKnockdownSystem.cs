using System.Linq;
using Content.Server._Goobstation.Wizard.Components;
using Content.Shared._EinsteinEngines.TelescopicBaton;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Timing;
using Robust.Server.Audio;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class UseDelayBlockKnockdownSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UseDelayBlockKnockdownComponent, KnockdownOnHitAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<UseDelayBlockKnockdownComponent, KnockdownOnHitSuccessEvent>(OnSuccess);
    }

    private void OnSuccess(Entity<UseDelayBlockKnockdownComponent> ent, ref KnockdownOnHitSuccessEvent args)
    {
        var (uid, comp) = ent;

        if (comp.ResetDelayOnSuccess)
            _delay.TryResetDelay(uid, id: comp.Delay);

        _audio.PlayPvs(comp.KnockdownSound, Transform(uid).Coordinates);

        if (!comp.DoSparks)
            return;
        foreach (var coords in args.KnockedDown.Select(knocked => Transform(knocked).Coordinates))
        {
            _sparks.DoSparks(coords, playSound: false);
        }
    }

    private void OnAttempt(Entity<UseDelayBlockKnockdownComponent> ent, ref KnockdownOnHitAttemptEvent args)
    {
        var (uid, comp) = ent;

        if (args.Cancelled || !TryComp(uid, out UseDelayComponent? delay))
            return;

        if (_delay.IsDelayed((uid, delay), comp.Delay))
            args.Cancelled = true;
    }
}
