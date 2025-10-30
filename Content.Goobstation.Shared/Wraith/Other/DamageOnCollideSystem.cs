using Content.Shared.Damage;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.Other;

public sealed class DamageOnCollideSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable  = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnCollideComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<DamageOnCollideComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnStartCollide(Entity<DamageOnCollideComponent> ent, ref StartCollideEvent args) =>
        _damageable.TryChangeDamage(ent.Owner, ent.Comp.Damage);

    private void OnPreventCollide(Entity<DamageOnCollideComponent> ent, ref PreventCollideEvent args)
    {
        if (_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.OtherEntity))
            return;

        if (_whitelist.IsWhitelistPass(ent.Comp.IgnoreWhitelist, args.OtherEntity))
            args.Cancelled = true;
    }
}
