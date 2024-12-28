using Content.Server.Temperature.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Damage.Components;

namespace Content.Server._Goobstation.Temperature;

public sealed class KillOnOverheatSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<KillOnOverheatComponent, TemperatureComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var comp, out var temp, out var mob))
        {
            if (mob.CurrentState == MobState.Dead
                || temp.CurrentTemperature < comp.OverheatThreshold
                || HasComp<GodmodeComponent>(uid))
                continue;

            var msg = Loc.GetString(comp.OverheatPopup, ("name", Identity.Name(uid, EntityManager)));
            _popup.PopupEntity(msg, uid, PopupType.LargeCaution);
            _mob.ChangeMobState(uid, MobState.Dead, mob);
        }
    }
}
