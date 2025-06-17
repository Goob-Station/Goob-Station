using System.Linq;
using Content.Goobstation.Common.Gun.Events;
using Content.Goobstation.Common.Style;
using Content.Goobstation.Shared.Dash;
using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.Sandevistan;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Electrocution;
using Content.Shared.Guardian;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Slippery;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Style
{
    public sealed class SharedStyleEventSystem : EntitySystem
    {
        [Dependency] private readonly StyleSystem _styleSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            // starwars intro wannabe
            SubscribeLocalEvent<StyleCounterComponent, MeleeAttackEvent>(OnMeleeAttack);
            SubscribeLocalEvent<StyleCounterComponent, DamageChangedEvent>(OnDamageDealt);
            SubscribeLocalEvent<StyleProjectileComponent, ProjectileHitEvent>(OnProjectileHit);
            SubscribeLocalEvent<StyleCounterComponent, SlipAttemptEvent>(OnSlipAttempt);
            SubscribeLocalEvent<StyleCounterComponent, UserShotAmmoEvent>(OnGunShot);
            SubscribeLocalEvent<StyleCounterComponent, ThrowHitByEvent>(OnThrowHitBy);
            SubscribeLocalEvent<StyleCounterComponent, ElectrocutedEvent>(OnElectrocuted);
            SubscribeLocalEvent<StyleCounterComponent, RejuvenateEvent>(OnRejuvenate);
            SubscribeLocalEvent<StyleCounterComponent, GuardianToggleActionEvent>(OnJojoReference);
            SubscribeLocalEvent<StyleCounterComponent, DashActionEvent>(On65);
            SubscribeLocalEvent<StyleCounterComponent, DisarmedEvent>(OnSkillIssue);
            SubscribeLocalEvent<StyleCounterComponent, TakeStaminaDamageEvent>(OnStaminaDamage);
            SubscribeLocalEvent<StyleCounterComponent, AnimationFlipEmoteEvent>(OnFlip);
            SubscribeLocalEvent<StyleCounterComponent, ToggleSandevistanEvent>(OnSandevistan);
        }

        private void OnSandevistan(EntityUid uid, StyleCounterComponent component, ToggleSandevistanEvent args)
        {
            component.CurrentPoints += 150;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "+FAST AF", component, Color.DarkGoldenrod);
        }

        private void OnFlip(EntityUid uid, StyleCounterComponent component, AnimationFlipEmoteEvent args)
        {
            component.CurrentPoints += 75;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "+FLIP", component, Color.Purple);
        }

        private void OnStaminaDamage(EntityUid uid, StyleCounterComponent component, TakeStaminaDamageEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted)
                return;
            component.CurrentPoints -= 100;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "-STUN DAMAGE", component, Color.Purple);
        }

        private void OnSkillIssue(EntityUid uid, StyleCounterComponent component, DisarmedEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted)
                return;
            component.CurrentPoints -= 200;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "-DISARMED", component, Color.Purple);
        }

        private void On65(EntityUid uid, StyleCounterComponent styleComp, DashActionEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted)
                return;

            styleComp.CurrentPoints += 65;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "+DASH", styleComp, Color.Purple);
        }

        private void OnJojoReference(EntityUid uid, StyleCounterComponent styleComp, GuardianToggleActionEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted)
                return;

            styleComp.CurrentPoints += 50;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "+JOJO REFERENCE", styleComp, Color.Purple); // Speedwagon foundation is proud
        }

        private void OnElectrocuted(EntityUid uid, StyleCounterComponent styleComp, ElectrocutedEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted || args.ShockDamage < 0 || !args.ShockDamage.HasValue)
                return;

            if (_net.IsServer)
            {
                styleComp.CurrentPoints -= 250; // massive skill issue
                RaiseLocalEvent(uid, new UpdateStyleEvent()); // ХУЛИ ОНО НАХУЙ НЕ РАБОТАЕТ СУКААААААААААААААААААААААААА
                _styleSystem.AddStyleEvent(uid, "-SHOCK", styleComp, Color.Red);
            }
        }

        private void OnRejuvenate(EntityUid uid, StyleCounterComponent styleComp, RejuvenateEvent args) // because i can.
        {
            if (!_gameTiming.IsFirstTimePredicted)
                return;

            styleComp.CurrentPoints += 200;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "+CHEATS", styleComp, Color.Green);
        }
        private void OnThrowHitBy(EntityUid uid, StyleCounterComponent styleComp, ThrowHitByEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted
                || TryComp<MobStateComponent>(uid, out var mobState))
                return;

            if (mobState != null && mobState.CurrentState == MobState.Alive)
            {
                styleComp.CurrentPoints -= 400;
                RaiseLocalEvent(uid, new UpdateStyleEvent());
                _styleSystem.AddStyleEvent(uid, "-OWNED", styleComp, Color.OrangeRed); // lmao
            }
        }

        private void OnMeleeAttack(EntityUid uid, StyleCounterComponent styleComp, MeleeAttackEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted
                || !args.HitEntities.Any())
                return;

            var validHits = args.HitEntities.Count(hit => TryComp<MobStateComponent>(hit, out var mobState) && mobState.CurrentState == MobState.Alive);

            if (validHits == 0)
                return;

            styleComp.CurrentPoints += 75;
            RaiseLocalEvent(uid, new UpdateStyleEvent());
            _styleSystem.AddStyleEvent(uid, "+MELEE HIT", styleComp, Color.LightGreen);
        }

        private void OnDamageDealt(EntityUid uid, StyleCounterComponent styleComp, DamageChangedEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted
                || args.DamageDelta == null)
                return;

            var totalDamage = args.DamageDelta.GetTotal();

            // Skip if damage is less than 2
            if (totalDamage < 2)
                return;

            if (totalDamage <= 12)
            {
                styleComp.CurrentPoints -= 100;
                RaiseLocalEvent(uid, new UpdateStyleEvent());
                _styleSystem.AddStyleEvent(uid, "-DAMAGE", styleComp, Color.Yellow);
            }
            else if (totalDamage <= 20)
            {
                styleComp.CurrentPoints -= 150;
                RaiseLocalEvent(uid, new UpdateStyleEvent());
                _styleSystem.AddStyleEvent(uid, "-MAJOR DAMAGE", styleComp, Color.Orange);
            }
            else
            {
                styleComp.CurrentPoints -= 300;
                RaiseLocalEvent(uid, new UpdateStyleEvent());
                _styleSystem.AddStyleEvent(uid, "-MAJOR DAMAGE", styleComp, Color.Red);
            }
        }

        private void OnProjectileHit(Entity<StyleProjectileComponent> ent, ref ProjectileHitEvent args)
        {
            if (!TryComp<MobStateComponent>(args.Target, out var mobState)
                || mobState.CurrentState != MobState.Alive
                || ent.Comp.Component == null
                || ent.Comp.User == null)
                return;

            if (_net.IsServer)
            {
                _styleSystem.AddStyleEvent(ent.Comp.User.Value, "+BULLET HIT", ent.Comp.Component, Color.BlueViolet);
                ent.Comp.Component.CurrentPoints += 125;
            }
            RaiseLocalEvent(ent.Comp.User.Value, new UpdateStyleEvent());
        }

        private void OnGunShot(EntityUid uid, StyleCounterComponent component, UserShotAmmoEvent args)
        {
            foreach (var projectile in args.FiredProjectiles)
            {
                var projectileUid = GetEntity(projectile);
                var comp = EnsureComp<StyleProjectileComponent>(projectileUid);
                comp.Component = component;
                comp.User = uid;
            }
        }

        private void OnSlipAttempt(EntityUid uid, StyleCounterComponent styleComp, SlipAttemptEvent args)
        {
            if (!_gameTiming.IsFirstTimePredicted)
                return;

            styleComp.CurrentPoints -= 400;
            _styleSystem.AddStyleEvent(uid, "-SLIP", styleComp, Color.Red);
        }
    }
}
