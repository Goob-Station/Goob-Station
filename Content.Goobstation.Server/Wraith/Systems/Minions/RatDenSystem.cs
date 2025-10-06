using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Robust.Shared.Timing;
namespace Content.Goobstation.Server.Wraith.Systems;

public sealed class RatDenSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RatDenComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<RatDenComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            // Spawns mad rat every X seconds.
            if (curTime >= comp.NextTickSpawn)
            {
                Spawn(comp.MadRatProto, Transform(uid).Coordinates);
                comp.NextTickSpawn = curTime + comp.TimeTillSpawn;
            }
            // Healing logic
            if (curTime >= comp.NextTickHeal)
            {
                var entities = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.HealRange);
                entities.RemoveWhere(entity => !HasComp<WraithMinionComponent>(entity));

                foreach (var entity in entities)
                {
                    _damage.TryChangeDamage(entity, comp.HealAmount, targetPart: TargetBodyPart.All);
                }

                comp.NextTickHeal = curTime + comp.HealCooldown;
            }
        }
    }
    private void OnMapInit(Entity<RatDenComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTickSpawn = _timing.CurTime + ent.Comp.TimeTillSpawn;
        ent.Comp.NextTickHeal = _timing.CurTime + ent.Comp.HealCooldown;
    }
}
