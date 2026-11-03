using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.Other;

public sealed partial class DamageOnCollideSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable  = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private TagSystem _tag = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnCollideComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<DamageOnCollideComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnStartCollide(Entity<DamageOnCollideComponent> ent, ref StartCollideEvent args)
    {
        var target = ent.Comp.Inverted ? args.OtherEntity : ent.Owner;
        _damageable.TryChangeDamage(target, ent.Comp.Damage);
    }

    private void OnPreventCollide(Entity<DamageOnCollideComponent> ent, ref PreventCollideEvent args)
    {
        if (_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.OtherEntity))
            return;

        if (_whitelist.IsWhitelistPass(ent.Comp.IgnoreWhitelist, args.OtherEntity))
            args.Cancelled = true;
    }
}
