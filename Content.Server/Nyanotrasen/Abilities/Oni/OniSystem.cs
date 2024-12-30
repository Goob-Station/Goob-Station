using Content.Server.Tools;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Tools.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Nyanotrasen.Abilities.Oni;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;

namespace Content.Server.Abilities.Oni
{
    public sealed class OniSystem : SharedOniSystem
    {
        [Dependency] private readonly ToolSystem _toolSystem = default!;
        [Dependency] private readonly GunSystem _gunSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<OniComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
            SubscribeLocalEvent<OniComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
            SubscribeLocalEvent<OniComponent, MeleeHitEvent>(OnOniMeleeHit);
            SubscribeLocalEvent<HeldByOniComponent, MeleeHitEvent>(OnHeldMeleeHit);
            SubscribeLocalEvent<HeldByOniComponent, StaminaMeleeHitEvent>(OnStamHit);
        }

        private void OnEntInserted(EntityUid uid, OniComponent component, EntInsertedIntoContainerMessage args)
        {
            var heldComp = EnsureComp<HeldByOniComponent>(args.Entity);
            heldComp.Holder = uid;

            if (TryComp<ToolComponent>(args.Entity, out var tool) && _toolSystem.HasQuality(args.Entity, "Prying", tool))
                tool.SpeedModifier *= 1.66f;

            if (_gunSystem.TryGetGun(args.Entity, out _, out var gun))
            {
                gun.MinAngle *= 15f;
                gun.AngleIncrease *= 15f;
                gun.MaxAngle *= 15f;
            }
        }

        private void OnEntRemoved(EntityUid uid, OniComponent component, EntRemovedFromContainerMessage args)
        {
            if (TryComp<ToolComponent>(args.Entity, out var tool) && _toolSystem.HasQuality(args.Entity, "Prying", tool))
                tool.SpeedModifier /= 1.66f;

            if (_gunSystem.TryGetGun(args.Entity, out _, out var gun))
            {
                gun.MinAngle /= 15f;
                gun.AngleIncrease /= 15f;
                gun.MaxAngle /= 15f;
            }

            RemComp<HeldByOniComponent>(args.Entity);
        }

        private void OnOniMeleeHit(EntityUid uid, OniComponent component, MeleeHitEvent args)
        {
            args.ModifiersList.Add(component.MeleeModifiers);
        }

        private void OnHeldMeleeHit(EntityUid uid, HeldByOniComponent component, MeleeHitEvent args)
        {
            if (!TryComp<OniComponent>(component.Holder, out var oni))
                return;

            args.ModifiersList.Add(oni.MeleeModifiers);
        }

        private void OnStamHit(EntityUid uid, HeldByOniComponent component, StaminaMeleeHitEvent args)
        {
            if (!TryComp<OniComponent>(component.Holder, out var oni))
                return;

            args.Multiplier *= oni.StamDamageMultiplier;
        }
    }
}
