using Content.Goobstation.Shared.BerserkerImplant;
using Content.Shared.Damage;

namespace Content.Goobstation.Server.BerserkerImplant;

public sealed class BerserkerImplantSystem : SharedBerserkerImplantSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BerserkerImplantActiveComponent, ComponentRemove>(OnShutdown);
    }

    private void OnShutdown(Entity<BerserkerImplantActiveComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.DelayedDamage.GetTotal() <= 0)
            return;

        if (TryComp<DamageableComponent>(ent, out var damageable))
        {
            _damageable.TryChangeDamage(ent.Owner, ent.Comp.DelayedDamage, true);
        }

        Popup.PopupEntity(Loc.GetString("berserker-implant-deactivated"), ent, ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = Timing.CurTime;
        var query = EntityQueryEnumerator<BerserkerImplantActiveComponent>();

        while (query.MoveNext(out var ent, out var berserker))
        {
            if (berserker.EndTime > curTime)
                continue;

            RemCompDeferred<BerserkerImplantActiveComponent>(ent);
        }
    }
}
