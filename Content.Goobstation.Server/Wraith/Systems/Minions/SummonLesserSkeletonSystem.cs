using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
namespace Content.Goobstation.Server.Wraith.Systems;

public sealed class SummonLesserSkeletonSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonLesserSkeletonComponent, SummonLesserSkeletonEvent>(OnSummon);
    }

    private void OnSummon(Entity<SummonLesserSkeletonComponent> ent, ref SummonLesserSkeletonEvent args)
    {
        if (args.Handled)
            return;

        Spawn(ent.Comp.SkeletonProto, Transform(ent.Owner).Coordinates);

        args.Handled = true;
    }
}
