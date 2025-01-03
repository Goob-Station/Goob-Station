using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared.Damage.Components;
using Content.Shared.Heretic;
using Content.Shared.Slippery;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.CombatMode.Pacification;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    private void SubscribeBlade()
    {
        SubscribeLocalEvent<HereticComponent, HereticCuttingEdgeEvent>(OnCuttingEdge);
        SubscribeLocalEvent<HereticComponent, ShotAttemptedEvent>(OnShootAttempt);

        SubscribeLocalEvent<HereticComponent, HereticDanceOfTheBrandEvent>(OnDanceOfTheBrand);
        SubscribeLocalEvent<HereticComponent, EventHereticRealignment>(OnRealignment);
        SubscribeLocalEvent<HereticComponent, HereticChampionStanceEvent>(OnChampionStance);
        SubscribeLocalEvent<HereticComponent, EventHereticFuriousSteel>(OnFuriousSteel);

        SubscribeLocalEvent<HereticComponent, HereticAscensionBladeEvent>(OnAscensionBlade);
    }

    private void OnCuttingEdge(Entity<HereticComponent> ent, ref HereticCuttingEdgeEvent args)
    {
        ent.Comp.CanShootGuns = false;
    }

    private void OnShootAttempt(Entity<HereticComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.CanShootGuns == false)
        {
            _popup.PopupEntity(Loc.GetString("heretic-cant-shoot", ("entity", args.Used)), ent, ent);
            args.Cancel();
        }
    }

    private void OnDanceOfTheBrand(Entity<HereticComponent> ent, ref HereticDanceOfTheBrandEvent args)
    {
        EnsureComp<RiposteeComponent>(ent);
    }
    
    private void OnRealignment(Entity<HereticComponent> ent, ref EventHereticRealignment args)
    {
        if (!TryUseAbility(ent, args))
            return;

        _statusEffect.TryRemoveStatusEffect(ent, "Stun");
        _statusEffect.TryRemoveStatusEffect(ent, "KnockedDown");
        _statusEffect.TryRemoveStatusEffect(ent, "ForcedSleep");
        _statusEffect.TryRemoveStatusEffect(ent, "Drowsiness");

        if (TryComp<StaminaComponent>(ent, out var stam))
        {
            if (stam.StaminaDamage >= stam.CritThreshold)
            {
                _stam.ExitStamCrit(ent, stam);
            }

            stam.StaminaDamage = 0;
            RemComp<ActiveStaminaComponent>(ent);
            Dirty(ent, stam);
        }

        _statusEffect.TryAddStatusEffect<PacifiedComponent>(ent, "Pacified", TimeSpan.FromSeconds(10f), true);

        args.Handled = true;
    }

    private void OnChampionStance(Entity<HereticComponent> ent, ref HereticChampionStanceEvent args)
    {
        // remove limbloss
        foreach (var part in _body.GetBodyChildren(ent))
            part.Component.CanSever = false;

        EnsureComp<ChampionStanceComponent>(ent);
    }
    private void OnFuriousSteel(Entity<HereticComponent> ent, ref EventHereticFuriousSteel args)
    {
        if (!TryUseAbility(ent, args))
            return;

        for (int i = 0; i < 3; i++)
            _pblade.AddProtectiveBlade(ent);

        args.Handled = true;
    }

    private void OnAscensionBlade(Entity<HereticComponent> ent, ref HereticAscensionBladeEvent args)
    {
        EnsureComp<NoSlipComponent>(ent); // epic gamer move
        RemComp<StaminaComponent>(ent); // no stun

        EnsureComp<SilverMaelstromComponent>(ent);
    }
}
