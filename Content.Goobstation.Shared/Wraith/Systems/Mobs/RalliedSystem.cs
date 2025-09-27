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
        SubscribeLocalEvent<RalliedComponent, ComponentShutdown>(OnShutdown);
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
        // Schedule deletion
        ent.Comp.NextTick = _timing.CurTime + ent.Comp.RalliedDuration;
        //TO DO: Increase attack speed.
        Dirty(ent);
    }
    private void OnShutdown(Entity<RalliedComponent> ent, ref ComponentShutdown args)
    {
        //TO DO: Restore original attack speed.
        _popup.PopupPredicted(
            Loc.GetString("rally-wears-off"),
            ent,
            ent,
            PopupType.MediumCaution);
    }

    private void OnGetMeleeDamage(Entity<RalliedComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        args.Damage *= ent.Comp.RalliedStrength;
    }
}
