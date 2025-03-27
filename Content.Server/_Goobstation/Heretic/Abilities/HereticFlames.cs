using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;

namespace Content.Server.Heretic.Abilities;

[RegisterComponent]
public sealed partial class HereticFlamesComponent : Component
{
    public float UpdateTimer = 0f;
    public float LifetimeTimer = 0f;
    [DataField] public float UpdateDuration = .2f;
    [DataField] public float LifetimeDuration = 60f;
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
            // remove it after ~60 seconds
            hfc.LifetimeTimer += frameTime;
            if (hfc.LifetimeTimer >= hfc.LifetimeDuration)
                RemCompDeferred(uid, hfc);

            // spawn fire box every .2 seconds
            hfc.UpdateTimer += frameTime;
            if (hfc.UpdateTimer >= hfc.UpdateDuration)
            {
                hfc.UpdateTimer = 0f;
                _has.SpawnFireBox(uid, 1, false);
            }
        }
    }
}
