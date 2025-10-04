using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class SummonPlagueRatSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonPlagueRatComponent, SummonPlagueRatEvent>(OnSummonRat);
    }

    private void OnSummonRat(Entity<SummonPlagueRatComponent> ent, ref SummonPlagueRatEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var xform = Transform(uid);

        if (args.Handled)
            return;

        if (_net.IsServer)
        {
            var voidUid = Spawn(comp.RatProto, xform.Coordinates);
        }

        args.Handled = true;
    }
}
