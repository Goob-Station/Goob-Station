using Content.Shared.Armor; // Goobstation - Armor resisting syringe gun
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chemistry.Components;
using Content.Shared.Chemistry.Components; // GoobStation
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Events;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Collections;

namespace Content.Server.Chemistry.EntitySystems;

/// <summary>
/// System for handling the different inheritors of <see cref="BaseSolutionInjectOnEventComponent"/>.
/// Subscribes to relevent events and performs solution injections when they are raised.
/// </summary>
public sealed class SolutionInjectOnCollideSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SolutionInjectOnProjectileHitComponent, ProjectileHitEvent>(HandleProjectileHit);
        SubscribeLocalEvent<SolutionInjectOnEmbedComponent, EmbedEvent>(HandleEmbed);
        SubscribeLocalEvent<MeleeChemicalInjectorComponent, MeleeHitEvent>(HandleMeleeHit);
        SubscribeLocalEvent<SolutionInjectWhileEmbeddedComponent, InjectOverTimeEvent>(OnInjectOverTime);

        SubscribeLocalEvent<SolutionInjectOnEmbedComponent, LandEvent>(OnEmbedLand);
        SubscribeLocalEvent<SolutionInjectWhileEmbeddedComponent, LandEvent>(OnWhileEmbeddedLand);
    }


    private void HandleProjectileHit(Entity<SolutionInjectOnProjectileHitComponent> entity, ref ProjectileHitEvent args)
    {
        DoInjection((entity.Owner, entity.Comp), args.Target, args.Shooter);
    }

    private void HandleEmbed(Entity<SolutionInjectOnEmbedComponent> entity, ref EmbedEvent args)
    {
        DoInjection((entity.Owner, entity.Comp), args.Embedded, args.Shooter);
    }

    private void HandleMeleeHit(Entity<MeleeChemicalInjectorComponent> entity, ref MeleeHitEvent args)
    {
        // MeleeHitEvent is weird, so we have to filter to make sure we actually
        // hit something and aren't just examining the weapon.
        if (args.IsHit)
            TryInjectTargets((entity.Owner, entity.Comp), args.HitEntities, args.User);
    }

    private void OnInjectOverTime(Entity<SolutionInjectWhileEmbeddedComponent> entity, ref InjectOverTimeEvent args)
    {
        DoInjection((entity.Owner, entity.Comp), args.EmbeddedIntoUid);
    }

    private void OnEmbedLand(Entity<SolutionInjectOnEmbedComponent> entity, ref LandEvent args)
    {
        ResetState(entity.Comp);
    }

    private void OnWhileEmbeddedLand(Entity<SolutionInjectWhileEmbeddedComponent> entity, ref LandEvent args)
    {
        ResetState(entity.Comp);
    }

    private void ResetState(BaseSolutionInjectOnEventComponent comp)
    {
        comp.PierceArmorOverride = null;
        comp.AmountMultiplier = 1f;
    }

    private void DoInjection(Entity<BaseSolutionInjectOnEventComponent> injectorEntity, EntityUid target, EntityUid? source = null)
    {
        TryInjectTargets(injectorEntity, [target], source);
    }

    /// <summary>
    /// Filters <paramref name="targets"/> for valid targets and tries to inject a portion of <see cref="BaseSolutionInjectOnEventComponent.Solution"/> into
    /// each valid target's bloodstream.
    /// </summary>
    /// <remarks>
    /// Targets are invalid if any of the following are true:
    /// <list type="bullet">
    ///     <item>The target does not have a bloodstream.</item>
    ///     <item><see cref="BaseSolutionInjectOnEventComponent.PierceArmor"/> is false and the target is wearing a hardsuit.</item>
    ///     <item><see cref="BaseSolutionInjectOnEventComponent.BlockSlots"/> is not NONE and the target has an item equipped in any of the specified slots.</item>
    /// </list>
    /// </remarks>
    /// <returns>true if at least one target was successfully injected, otherwise false</returns>
    private bool TryInjectTargets(Entity<BaseSolutionInjectOnEventComponent> injector, IReadOnlyList<EntityUid> targets, EntityUid? source = null)
    {
        // Make sure we have at least one target
        if (targets.Count == 0)
            return false;

        // Get the solution to inject
        if (!_solutionContainer.TryGetSolution(injector.Owner, injector.Comp.Solution, out var injectorSolution))
            return false;

        bool anySuccess = false;

        foreach (var target in targets)
        {
            if (Deleted(target))
                continue;

            // Goobstation - Armor resisting syringe gun
            var mult = injector.Comp.AmountMultiplier; // multiplier of how much to actually inject
            var pierce = injector.Comp.PierceArmorOverride ?? injector.Comp.PierceArmor;
            if (_inventory.TryGetSlotEntity(target, "outerClothing", out var suit)) // attempt to apply armor injection speed multiplier or block the syringe
            {
                bool syringeArmor = _tag.HasTag(suit.Value, "SyringeArmor");
                bool blocked = syringeArmor && !pierce; // if we have syringe armor and it's not piercing just block it outright
                pierce = pierce && !syringeArmor; // if we have syringe armor and it IS piercing, downgrade it

                if (!blocked && !pierce && TryComp<ArmorComponent>(suit, out var armor)) // don't bother checking if we already blocked
                {
                    var modifierDict = injector.Comp.DamageModifierResistances;
                    var armorCoefficients = armor.Modifiers.Coefficients;
                    foreach (var coefficient in modifierDict)
                    {
                        if (armorCoefficients.ContainsKey(coefficient.Key))
                        {
                            mult *= 1f - (1f - armorCoefficients[coefficient.Key]) * coefficient.Value;
                        }
                    }
                    if (mult <= 0f)
                        blocked = true;
                }
                if (blocked)
                {
                    // Only show popup to attacker
                    if (source != null)
                        _popup.PopupEntity(Loc.GetString(injector.Comp.BlockedByArmorPopupMessage, ("weapon", injector.Owner), ("target", target)), target, source.Value, PopupType.SmallCaution);

                    continue;
                }
            }

            // Check if the target has anything equipped in a slot that would block injection
            if (injector.Comp.BlockSlots != SlotFlags.NONE)
            {
                var blocked = false;
                var containerEnumerator = _inventory.GetSlotEnumerator(target, injector.Comp.BlockSlots);
                while (containerEnumerator.MoveNext(out var container))
                {
                    if (container.ContainedEntity != null)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked)
                    continue;
            }

            // Make sure the target has a bloodstream
            if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
                continue;

            Solution removedSolution = _solutionContainer.SplitSolution(injectorSolution.Value, injector.Comp.TransferAmount * mult);
            // Adjust solution amount based on transfer efficiency
            var solutionToInject = removedSolution.SplitSolution(removedSolution.Volume * injector.Comp.TransferEfficiency);
            // Inject our portion into the target's bloodstream
            if (_bloodstream.TryAddToChemicals(target, solutionToInject, bloodstream))
                anySuccess = true;
        }
        // Goobstation - Armor resisting syringe gun
        // on upstream there would be code here but it migrates north in the goobstation season

        // Huzzah!
        return anySuccess;
    }
}
