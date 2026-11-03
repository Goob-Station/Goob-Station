using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModCloakModuleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModCloakModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (args.Wearer is not { } wearer)
            return;

        var cloaked = EnsureComp<ModCloakedComponent>(wearer);
        cloaked.Module = ent;
        Dirty(wearer, cloaked);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModCloakModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (args.Wearer is { } wearer
            && TryComp<ModCloakedComponent>(wearer, out var cloaked)
            && cloaked.Module == ent.Owner)
            RemComp(wearer, cloaked);
    }

    [SubscribeLocalEvent]
    private void OnMeleeAttack(Entity<ModCloakedComponent> ent, ref MeleeAttackEvent args)
    {
        BreakCloak(ent);
    }

    [SubscribeLocalEvent]
    private void OnGunShot(Entity<ModCloakedComponent> ent, ref SelfBeforeGunShotEvent args)
    {
        BreakCloak(ent);
    }

    [SubscribeLocalEvent]
    private void OnDamaged(Entity<ModCloakedComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageIncreased && args.Origin != null && args.Origin != ent.Owner)
            BreakCloak(ent);
    }

    private void BreakCloak(Entity<ModCloakedComponent> ent)
    {
        if (_timing.ApplyingState
            || ent.Comp.Module is not { } module
            || !TryComp<ModModuleComponent>(module, out var moduleComp)
            || !moduleComp.Active
            || !_modSuit.TryGetModuleSuit(module, out var suit))
            return;

        _modSuit.SetModuleActive(suit, (module, moduleComp), false);
        _popup.PopupClient(Loc.GetString("modsuit-cloak-broken"), ent, ent, PopupType.SmallCaution);
    }
}
