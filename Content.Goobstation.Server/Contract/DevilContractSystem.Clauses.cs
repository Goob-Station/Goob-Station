using System.Linq;
using Content.Goobstation.Common.Traits;
using Content.Goobstation.Server.Condemned;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.Devil;
using Content.Server._Shitmed.DelayedDeath;
using Content.Server.Access.Systems;
using Content.Server.Body.Systems;
using Content.Server.Speech.Components;
using Content.Shared.Access.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Clumsy;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Popups;
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
                    devil.Souls++;
                    _popupSystem.PopupEntity(
                        Loc.GetString("contract-soul-added"),
                        (EntityUid)contract.ContractOwner,
                        PopupType.LargeCaution);

                }
            },

            // Cuts all damage taken in half.
            ["weakness"] = (target, contract) =>
            {
                _damageable.SetDamageModifierSetId(target, "DevilDealPositive");

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

            // Paralyzes both legs
            ["legs"] = (target, contract) =>
            {
                EnsureComp<LegsParalyzedComponent>(target);
            },

            // Grants one free revive on death.
            ["death"] = (target, contract) =>
            {
                EnsureComp<CheatDeathComponent>(target, out var cheatDeathComponent);
                cheatDeathComponent.ReviveAmount = 1;
            },

            // Grants an infinite amount of revives.
            ["death everlasting"] = (target, contract) =>
            {
                EnsureComp<CheatDeathComponent>(target, out var cheatDeathComponent);
                cheatDeathComponent.ReviveAmount = -1;
            },

            // Grants all-access // Todo - Implement
            ["locks"] = (target, contract) =>
            {
            },

            // Kills the target after five minutes
            ["time"] = (target, contract) =>
            {
                EnsureComp<DelayedDeathComponent>(target, out var delayedDeathComponent);
                EnsureComp<UnrevivableComponent>(target);
                delayedDeathComponent.DeathTime = 300;
                delayedDeathComponent.DeathMessageId = "devil-deal-time-ran-out";
            },
        };
    }
}
