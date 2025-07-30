using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Actions.Events;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class MassSleepPowerSystem : EntitySystem
    {
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MassSleepPowerComponent, MassSleepPowerActionEvent>(OnPowerUsed);
        }

        private void OnPowerUsed(EntityUid uid, MassSleepPowerComponent component, MassSleepPowerActionEvent args)

        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, "mass sleep", true))
                return;

            foreach (var entity in _lookup.GetEntitiesInRange(args.Target, component.Radius))
            {
                if (HasComp<MobStateComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity))
                {
                    if (TryComp<DamageableComponent>(entity, out var damageable) && damageable.DamageContainerID == "Biological")
                        EnsureComp<SleepingComponent>(entity);
                }
            }
            _psionics.LogPowerUsed(uid, "mass sleep");
            args.Handled = true;
        }
    }
}
