using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Popups;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;

public sealed partial class RalliedSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RalliedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RalliedComponent, GetUserMeleeDamageEvent>(OnGetMeleeDamage);
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<RalliedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime >= comp.NextTick)
            {
                RemCompDeferred<RalliedComponent>(uid);
            }
        }
    }
    private void OnMapInit(Entity<RalliedComponent> ent, ref MapInitEvent args)
    {
        //TO DO: Increase attack speed.
        //TO DO: Add a popup that doesn't spam itself infinitely for no reason (On update), or one that doesn't instantly get called on Init even though it is in a shutdown event (OnShUtdown).
        Dirty(ent);
    }

    private void OnGetMeleeDamage(Entity<RalliedComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        args.Damage *= ent.Comp.RalliedStrength;
    }
}
