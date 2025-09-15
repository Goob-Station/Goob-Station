using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class SummonVoidCreatureSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonVoidCreatureComponent, SummonVoidCreatureEvent>(OnSummonVoidCreature);
    }

    private void OnSummonVoidCreature(Entity<SummonVoidCreatureComponent> ent, ref SummonVoidCreatureEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var xform = Transform(uid);

        if (args.Handled)
            return;


        //TO DO: Allow the wraith to pick which of the three creatures to spawn.
        if (_net.IsServer)
        {
            var randomNumber = Random.Shared.Next(1, 4);
            switch (randomNumber)
            {
                case 1:
                    {
                        var voidUid = Spawn(comp.CommanderProto, xform.Coordinates);
                        break;
                    }
                case 2:
                    {
                        var voidUid = Spawn(comp.FiendProto, xform.Coordinates);
                        break;
                    }
                case 3:
                    {
                        var voidUid = Spawn(comp.HoundProto, xform.Coordinates);
                        break;
                    }
                default:
                    break;
            }
        }

        args.Handled = true;
    }
}
