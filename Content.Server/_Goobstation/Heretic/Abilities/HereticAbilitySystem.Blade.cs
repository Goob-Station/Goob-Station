using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared.Body.Part;
using Content.Shared.Damage.Components;
using Content.Shared.Heretic;
using Content.Shared.Slippery;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    private void SubscribeBlade()
    {
        SubscribeLocalEvent<HereticComponent, HereticDanceOfTheBrandEvent>(OnDanceOfTheBrand);
        SubscribeLocalEvent<HereticComponent, EventHereticRealignment>(OnRealignment);
        SubscribeLocalEvent<HereticComponent, HereticChampionStanceEvent>(OnChampionStance);
        SubscribeLocalEvent<HereticComponent, EventHereticFuriousSteel>(OnFuriousSteel);

        SubscribeLocalEvent<HereticComponent, HereticAscensionBladeEvent>(OnAscensionBlade);
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

        _statusEffect.TryAddStatusEffect(ent, "Pacified", TimeSpan.FromSeconds(10f), true);

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
