using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;

namespace Content.Goobstation.Server.Wraith.Systems.Minions;

/// <summary>
/// Spawns a rat den, deletes previous one if it already exists.
/// </summary>
public sealed class SummonRatDenSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonRatDenComponent, SummonRatDenEvent>(OnSummonRatDen);
    }

    private void OnSummonRatDen(Entity<SummonRatDenComponent> ent, ref SummonRatDenEvent args)
    {
        if (ent.Comp.RatDenUid == null)
        {
            ent.Comp.RatDenUid = SpawnAtPosition(ent.Comp.RatDen, args.Target);
            args.Handled = true;
            return;
        }

        QueueDel(ent.Comp.RatDenUid); // delete previous rat den
        ent.Comp.RatDenUid = SpawnAtPosition(ent.Comp.RatDen, args.Target); // spawn new one
        args.Handled = true;
    }
}
