using Content.Goobstation.Shared.Voodoo;
using Content.Shared.Body.Systems;
using Content.Server.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Throwing;
using Robust.Shared.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Server.Player;
using Robust.Shared.Random;
using Robust.Shared.Maths;
using System.Numerics;

namespace Content.Goobstation.Server.Voodoo
{
    /// <summary>
    /// System used for the voodoo component for making someone take damage, and throw them when their counterpart "voodoo doll" takes damage.
    /// </summary>
    public sealed class VengeanceSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly SharedBodySystem _bodySystem = default!;
        [Dependency] private readonly ThrowingSystem _throwing = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<VoodooComponent, DamageChangedEvent>(OnDamaged);
            SubscribeLocalEvent<VoodooComponent, DestructionEventArgs>(OnDestroyed);
            SubscribeLocalEvent<VoodooComponent, ThrownEvent>(OnThrow);
        }

        private void OnDamaged(EntityUid uid, VoodooComponent comp, ref DamageChangedEvent args)
        {
            if (string.IsNullOrWhiteSpace(comp.TargetName))
                return;

            foreach (var session in _playerManager.Sessions)
            {
                if (session.AttachedEntity is not { Valid: true } target)
                    continue;

                var name = MetaData(target).EntityName;

                if (!name.Equals(comp.TargetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var bruteGroup = _proto.Index<DamageGroupPrototype>("Brute");
                var damage = new DamageSpecifier(bruteGroup, 10);

                _damageable.TryChangeDamage(target, damage);

                break;
            }
        }

        private void OnDestroyed(EntityUid uid, VoodooComponent comp, DestructionEventArgs args)
        {
            foreach (var session in _playerManager.Sessions)
            {
                if (session.AttachedEntity is not { Valid: true } target)
                    continue;

                var name = MetaData(target).EntityName;

                if (!name.Equals(comp.TargetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var bruteGroup = _proto.Index<DamageGroupPrototype>("Brute");
                var damage = new DamageSpecifier(bruteGroup, 200);

                _damageable.TryChangeDamage(target, damage);

                break;
            }
        }
        private void OnThrow(Entity<VoodooComponent> ent, ref ThrownEvent args)
        {
            foreach (var session in _playerManager.Sessions)
            {
                if (session.AttachedEntity is not { Valid: true } target)
                    continue;

                var name = MetaData(target).EntityName;

                if (!name.Equals(ent.Comp.TargetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var random = IoCManager.Resolve<IRobustRandom>();
                var strength = random.NextFloat(3f, 5f);

                var origin = Transform(ent).MapPosition.Position;
                var targetPos = Transform(target).MapPosition.Position;

                var direction = targetPos - origin;
                if (direction == Vector2.Zero)
                    direction = random.NextVector2(1f);

                _throwing.TryThrow(target, direction, strength, args.User);


                break;
            }
        }
    }
}
