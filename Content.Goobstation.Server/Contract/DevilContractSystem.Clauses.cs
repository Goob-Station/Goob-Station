using System.Linq;
using Content.Goobstation.Common.Traits;
using Content.Goobstation.Server.Condemned;
using Content.Goobstation.Server.Devil;
using Content.Goobstation.Server.Insanity;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.Devil;
using Content.Server._Shitmed.DelayedDeath;
using Content.Server.Access.Systems;
using Content.Server.Atmos;
using Content.Server.Atmos.Components;
using Content.Server.Body.Systems;
using Content.Server.Flash.Components;
using Content.Server.NPC.Queries.Considerations;
using Content.Server.Speech.Components;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._vg.TileMovement;
using Content.Shared.Access.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Clumsy;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Electrocution;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.Traits.Assorted;

namespace Content.Goobstation.Server.Contract;

public sealed partial class DevilContractSystem
{
    public DevilContractSystem()
    {
        ContractClauses = new Dictionary<string, Action<EntityUid, DevilContractComponent>>(StringComparer.OrdinalIgnoreCase)
        {
            // If you die with no soul, you go to hell!!
            ["soul ownership"] = (target, contract) =>
            {
                var soul = EnsureComp<CondemnedComponent>(target);
                soul.SoulOwner = contract.ContractOwner;

                if (TryComp<DevilComponent>(contract.ContractOwner, out var devil))
                {
                    _popupSystem.PopupEntity(
                        Loc.GetString("contract-soul-added"),
                        (EntityUid)contract.ContractOwner,
                        PopupType.LargeCaution);

                    // Transfer soul
                    TryTransferSouls(contract.ContractOwner, target, 1);
                }
            },

            // Cuts all damage taken in half.
            ["weakness"] = (target, contract) =>
            {
                _damageable.SetDamageModifierSetId(target, "DevilDealPositive");
            },

            // Makes the target immune to fire
            ["fear of fire"] = (target, contract) =>
            {
                _damageable.SetDamageModifierSetId(target, "HellSpawn");
                RemComp<FlammableComponent>(target);
            },

            // Makes the target immune to spacing and not need to breath.
            ["fear of space"] = (target, contract) =>
            {
                EnsureComp<BreathingImmunityComponent>(target);
                EnsureComp<PressureImmunityComponent>(target);
            },

            // Makes the target immune to flashes
            ["fear of light"] = (target, contract) =>
            {
                EnsureComp<FlashImmunityComponent>(target);
            },

            // Makes the target immune to electricity.
            ["fear of electricity"] = (target, contract) =>
            {
                EnsureComp<InsulatedComponent>(target);
            },

            // Doubles all damage taken.
            ["strength"] = (target, contract) =>
            {
                _damageable.SetDamageModifierSetId(target, "DevilDealNegative");
            },

            // Pacifies the target
            ["will to fight"] = (target, contract) =>
            {
                EnsureComp<PacifiedComponent>(target);
            },

            // Blinds the target
            ["sight"] = (target, contract) =>
            {
                EnsureComp<PermanentBlindnessComponent>(target);
            },

            // Takes a hand
            ["a hand"] = (target, contract) =>
            {
                var bodySystem = _entityManager.System<BodySystem>();
                TryComp<BodyComponent>(target, out var body);
                var hand = bodySystem.GetBodyChildrenOfType(target, BodyPartType.Hand, body).FirstOrDefault();
                _transform.AttachToGridOrMap(hand.Id);
            },

            // Takes a leg
            ["a leg"] = (target, contract) =>
            {
                var bodySystem = _entityManager.System<BodySystem>();
                TryComp<BodyComponent>(target, out var body);
                var leg = bodySystem.GetBodyChildrenOfType(target, BodyPartType.Leg, body).FirstOrDefault();
                _transform.AttachToGridOrMap(leg.Id);
            },

            // Takes an organ
            ["an organ"] = (target, contract) =>
            {
                var bodySystem = _entityManager.System<BodySystem>();
                TryComp<BodyComponent>(target, out var body);
                var organ = bodySystem.GetBodyOrgans(target).FirstOrDefault();
                _transform.AttachToGridOrMap(organ.Id);
            },

            // Effectively Lobotomizes you
            ["coherence"] = (target, contract) =>
            {
                EnsureComp<BackwardsAccentComponent>(target);
                EnsureComp<ClumsyComponent>(target);
            },

            // Mutes you
            ["voice"] = (target, contract) =>
            {
                EnsureComp<MutedComponent>(target);
            },

            // Makes you fucking crazy!!
            ["sanity"] = (target, contract) =>
            {
                EnsureComp<InsanityComponent>(target);
            },

            // Makes you walk with inverted controls
            ["stability"] = (target, contract) =>
            {
                TryComp<MovementSpeedModifierComponent>(target, out var movement);
            },

            // Gives you tile movement
            ["inner peace"] = (target, contract) =>
            {
                EnsureComp<TileMovementComponent>(target);
            },

            // Paralyzes both legs
            ["legs"] = (target, contract) =>
            {
                EnsureComp<LegsParalyzedComponent>(target);
            },

            // Grants one free revive on death.
            ["death"] = (target, contract) =>
            {
                EnsureComp<CheatDeathComponent>(target, out var cheatDeathComponent);
                cheatDeathComponent.ReviveAmount++;
                _rejuvenateSystem.PerformRejuvenate(target);
            },

            // Grants an infinite amount of revives.
            ["mortality"] = (target, contract) =>
            {
                EnsureComp<CheatDeathComponent>(target, out var cheatDeathComponent);
                cheatDeathComponent.ReviveAmount = -1;
            },

            // Kills the target after five minutes
            ["time"] = (target, contract) =>
            {
                // Can't cheat this one pal.
                RemComp<CheatDeathComponent>(target);
                EnsureComp<DelayedDeathComponent>(target, out var delayedDeathComponent);
                EnsureComp<UnrevivableComponent>(target);
                delayedDeathComponent.DeathTime = 300;
                delayedDeathComponent.DeathMessageId = "devil-deal-time-ran-out";
            },
        };
    }
}
