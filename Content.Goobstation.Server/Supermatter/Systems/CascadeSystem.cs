using Content.Goobstation.Shared.Supermatter.Components;
using Content.Server.Spreader;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Server.Supermatter.Systems;

public sealed class CascadeSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CascadeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CascadeComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, CascadeComponent component, StartCollideEvent args)
    {
        _bodySystem.GibBody(uid);
    }

    private void OnMapInit(EntityUid uid, CascadeComponent component, MapInitEvent args)
    {
            foreach (var ent in _lookup.GetEntitiesInRange(uid, component.Radius))
            {
                if (ent == uid)
                    continue;

                if (HasComp<CascadeComponent>(ent))
                    continue;
                _damageable.TryChangeDamage(ent, component.Damage, origin: uid);
            }
    }
}
