using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
namespace Content.Goobstation.Server.Wraith.Systems;

public sealed class SummonRatDenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonRatDenComponent, SummonRatDenEvent>(OnSummon);
    }

    private void OnSummon(Entity<SummonRatDenComponent> ent, ref SummonRatDenEvent args)
    {
        if (args.Handled)
            return;

        Spawn(ent.Comp.RatDenProto, Transform(ent.Owner).Coordinates);

        args.Handled = true;
    }
}
