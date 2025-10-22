using Content.Goobstation.Shared.Voodoo;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Throwing;
using Robust.Shared.Prototypes;
using Robust.Server.Player;
using Robust.Shared.Random;
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
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<VoodooComponent, DamageChangedEvent>(OnDamaged);
            SubscribeLocalEvent<VoodooComponent, DestructionEventArgs>(OnDestroyed);
            SubscribeLocalEvent<VoodooComponent, ThrownEvent>(OnThrow);
        }

        /// <summary>
        /// When the voodoo item takes damage, deal damage to the player
        /// </summary>
        private void OnDamaged(EntityUid uid, VoodooComponent comp, ref DamageChangedEvent args)
        {
            if (string.IsNullOrWhiteSpace(comp.TargetName))
                return;

            var damageType = _proto.Index(comp.DamageType);
            var damage = new DamageSpecifier(damageType, comp.Damage);

            foreach (var session in _playerManager.Sessions)
            {
                if (session.AttachedEntity is not { Valid: true } target)
                    continue;

                var name = MetaData(target).EntityName;

                if (!name.Equals(comp.TargetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                _damageable.TryChangeDamage(target, damage);
            }
        }

        /// <summary>
        /// When the voodoo item is destroyed, deal 200 blunt
        /// </summary>
        private void OnDestroyed(EntityUid uid, VoodooComponent comp, DestructionEventArgs args)
        {

            var damageType = _proto.Index(comp.DamageType);
            var damage = new DamageSpecifier(damageType, comp.DamageOnDestroy);

            foreach (var session in _playerManager.Sessions)
            {
                if (session.AttachedEntity is not { Valid: true } target)
                    continue;

                var name = MetaData(target).EntityName;

                if (!name.Equals(comp.TargetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (comp.GibOnDestory)
                {
                    _bodySystem.GibBody(target, splatModifier: 20);
                }
                else
                {
                    _damageable.TryChangeDamage(target, damage);
                }
            }
        }

        /// <summary>
        /// Whenever the voodoo item is thrown, throw the player in a random direction
        /// </summary>
        private void OnThrow(Entity<VoodooComponent> ent, ref ThrownEvent args)
        {
            foreach (var session in _playerManager.Sessions)
            {
                if (session.AttachedEntity is not { Valid: true } target)
                    continue;

                var name = MetaData(target).EntityName;

                if (!name.Equals(ent.Comp.TargetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var strength = _random.NextFloat(3f, 5f);

                var origin = Transform(ent).MapPosition.Position;
                var targetPos = Transform(target).MapPosition.Position;

                var direction = targetPos - origin;
                if (direction == Vector2.Zero)
                    direction = _random.NextVector2(1f);

                _throwing.TryThrow(target, direction, strength, args.User);
            }
        }
    }
}
