using Content.Shared._Funkystation.Explosion.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Funkystation.Explosion.EntitySystems;

public sealed partial class ExplosionProjectileCollisionSystem : EntitySystem
{
    [Dependency] private INetManager _net = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeleteOnCollideComponent, StartCollideEvent>(OnDeleteOnCollideStartCollide);
    }

    private void OnDeleteOnCollideStartCollide(Entity<DeleteOnCollideComponent> ent, ref StartCollideEvent args)
    {
        if (_net.IsServer)
            QueueDel(ent);
    }
}
