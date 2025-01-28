using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;

namespace Content.Server.Heretic.Abilities;

[RegisterComponent]
public sealed partial class HereticFlamesComponent : Component
{
    public float Timer = 0f;
    public float TimerSeconds = 0f;
    public float UpdateDuration = .2f;
    [DataField] public float Duration = 60f;
}

public sealed partial class HereticFlamesSystem : EntitySystem
{
    [Dependency] private readonly HereticAbilitySystem _has = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HereticFlamesComponent>();
        while (eqe.MoveNext(out var uid, out var hfc))
        {
            hfc.Timer += frameTime;
            if (hfc.Timer < hfc.UpdateDuration)
                continue;

            hfc.Timer = 0f;
            hfc.TimerSeconds += 1f;

            if (hfc.TimerSeconds >= hfc.Duration)
                RemComp(uid, hfc);

            _has.SpawnDamageBox(uid, 1, false);
        }
    }
}
