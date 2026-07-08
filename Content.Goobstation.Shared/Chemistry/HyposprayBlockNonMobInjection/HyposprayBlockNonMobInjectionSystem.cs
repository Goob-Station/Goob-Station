// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Chemistry.HyposprayBlockNonMobInjection;

public sealed class HyposprayBlockNonMobInjectionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HyposprayBlockNonMobInjectionComponent, AfterInteractEvent>(OnAfterInteract, before: new []{typeof(InjectorSystem)});
        SubscribeLocalEvent<HyposprayBlockNonMobInjectionComponent, MeleeHitEvent>(OnAttack, before: new []{typeof(InjectorSystem)});
        SubscribeLocalEvent<HyposprayBlockNonMobInjectionComponent, UseInHandEvent>(OnUseInHand, before: new []{typeof(InjectorSystem)});
    }

    private void OnUseInHand(Entity<HyposprayBlockNonMobInjectionComponent> ent, ref UseInHandEvent args)
    {
        if (!IsMob(args.User))
            args.Handled = true;
    }

    private void OnAttack(Entity<HyposprayBlockNonMobInjectionComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0 || !IsMob(args.HitEntities[0]))
            args.Handled = true;
    }

    private void OnAfterInteract(Entity<HyposprayBlockNonMobInjectionComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !IsMob(args.Target.Value))
            args.Handled = true;
    }

    private bool IsMob(EntityUid uid)
    {
        return HasComp<MobStateComponent>(uid);
    }
}
