using Content.Goobstation.Shared.Voodoo;
using Content.Shared.Body.Systems;
using Content.Server.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible;
using Robust.Shared.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Server.Player;

namespace Content.Goobstation.Server.Voodoo
{
    /// <summary>
    /// System used for the voodoo component for making someone take damage when their counterpart "voodoo doll" takes damage.
    /// </summary>
    public sealed class VengeanceSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly SharedBodySystem _bodySystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            // Listen for when an entity with VengeanceComponent is damaged
            SubscribeLocalEvent<VoodooComponent, DamageChangedEvent>(OnDamaged);
            SubscribeLocalEvent<VoodooComponent, DestructionEventArgs>(OnDestroyed);
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

                _bodySystem.GibBody(target, splatModifier: 20);

                break;
            }
        }
    }
}
