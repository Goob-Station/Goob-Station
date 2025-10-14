using Content.Server.Stunnable;
using Content.Shared._CorvaxGoob.Weapons.Misc;
using Content.Shared.StatusEffect;

namespace Content.Server._CorvaxGoob.Weapons.Misc;

public sealed class GrapplingGunHunterSystem : SharedGrapplingGunHunterSystem
{
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GrapplingHookedHunterComponent, GrapplingHookHunterStunEvent>(OnGrapplingHookHunterStun);
    }

    private void OnGrapplingHookHunterStun(EntityUid uid, GrapplingHookedHunterComponent component, GrapplingHookHunterStunEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<StatusEffectsComponent>(uid, out var status))
            return;

        _stun.TryParalyze(uid, args.Duration, true, status);
        args.Handled = true;
    }
}
