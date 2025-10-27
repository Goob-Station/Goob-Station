using Content.Shared.Damage;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.Other;

public sealed class DamageOnCollideSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable  = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnCollideComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(Entity<DamageOnCollideComponent> ent, ref StartCollideEvent args) =>
        _damageable.TryChangeDamage(ent.Owner, ent.Comp.Damage);
}
