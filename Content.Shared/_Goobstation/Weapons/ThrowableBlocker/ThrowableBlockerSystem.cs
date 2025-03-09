using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Goobstation.Weapons.ThrowableBlocker;

public sealed class ThrowableBlockerSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        // This uses ReflectUserComponent because I'm too lazy
        SubscribeLocalEvent<ReflectUserComponent, ThrowHitByEvent>(OnThrowHit,
            before: new[] { typeof(SharedCreamPieSystem) });
    }

    private void OnThrowHit(Entity<ReflectUserComponent> ent, ref ThrowHitByEvent args)
    {
        var thrown = args.Thrown;

        if (!TryComp(thrown, out ThrowableBlockedComponent? blockedComp))
            return;

        var blocked = _hands
            .EnumerateHeld(ent)
            .FirstOrDefault(e => HasComp<ThrowableBlockerComponent>(e) && _toggle.IsActivated(e));

        if (blocked == default)
            return;

        var blockerComp = Comp<ThrowableBlockerComponent>(blocked);

        args.Handled = true;

        if (_net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("throwable-blocker-blocked"), ent);
            _audio.PlayPvs(blockerComp.BlockSound, ent);
        }

        switch (blockedComp.Behavior)
        {
            case BlockBehavior.KnockOff:
                Knockoff(thrown);
                break;
            case BlockBehavior.Damage:
                Knockoff(thrown);
                _damageable.TryChangeDamage(thrown, blockedComp.Damage);
                break;
            case BlockBehavior.Destroy when _net.IsServer:
                Del(thrown);
                break;
        }
    }

    private void Knockoff(EntityUid entity)
    {
        if (!TryComp(entity, out PhysicsComponent? physics) || physics.LinearVelocity.LengthSquared() <= 0f)
            return;

        _physics.SetLinearVelocity(entity, -physics.LinearVelocity / 3f, body: physics);
    }
}
