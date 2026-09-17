// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Verbs;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

// Shitmed Change
using System.Linq;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Localizations;

namespace Content.Shared.Armor;

/// <summary>
///     This handles logic relating to <see cref="ArmorComponent" />
/// </summary>
public abstract class SharedArmorSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnRelayDamageModify);
        SubscribeLocalEvent<ArmorComponent, InventoryRelayedEvent<CoefficientQueryEvent>>(OnCoefficientQuery);
        SubscribeLocalEvent<ArmorComponent, BorgModuleRelayedEvent<DamageModifyEvent>>(OnBorgDamageModify);
        SubscribeLocalEvent<ArmorComponent, GetVerbsEvent<ExamineVerb>>(OnArmorVerbExamine);
    }

    private void OnDamageModify(EntityUid uid, ArmorComponent component, DamageModifyEvent args)
    {
        if (args.TargetPart == null)
            return;

        var (partType, _) = _body.ConvertTargetBodyPart(args.TargetPart);

        if (component.ArmorCoverage.Contains(partType))
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage,
            DamageSpecifier.PenetrateArmor(component.Modifiers, args.Damage.ArmorPenetration));
    }

    /// <summary>
    /// Get the total Damage reduction value of all equipment caught by the relay.
    /// </summary>
    /// <param name="ent">The item that's being relayed to</param>
    /// <param name="args">The event, contains the running count of armor percentage as a coefficient</param>
    private void OnCoefficientQuery(Entity<ArmorComponent> ent, ref InventoryRelayedEvent<CoefficientQueryEvent> args)
    {
        if (TryComp<MaskComponent>(ent, out var mask) && mask.IsToggled)
            return;

        foreach (var armorCoefficient in ent.Comp.Modifiers.Coefficients)
        {
            args.Args.DamageModifiers.Coefficients[armorCoefficient.Key] = args.Args.DamageModifiers.Coefficients.TryGetValue(armorCoefficient.Key, out var coefficient) ? coefficient * armorCoefficient.Value : armorCoefficient.Value;
        }
    }

    private void OnRelayDamageModify(EntityUid uid, ArmorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if (TryComp<MaskComponent>(uid, out var mask) && mask.IsToggled)
            return;

        // Goob-Fix-Start
        if (args.Args.TargetPart == null)
            return;

        var (partType, _) = _body.ConvertTargetBodyPart(args.Args.TargetPart);

        if (component.ArmorCoverage.Contains(partType))
            args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage,
            DamageSpecifier.PenetrateArmor(component.Modifiers, args.Args.Damage.ArmorPenetration));
        // Goob-Fix-End
    }

    private void OnBorgDamageModify(EntityUid uid, ArmorComponent component,
        ref BorgModuleRelayedEvent<DamageModifyEvent> args)
    {
        if (TryComp<MaskComponent>(uid, out var mask) && mask.IsToggled)
            return;

        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage,
        DamageSpecifier.PenetrateArmor(component.Modifiers, args.Args.Damage.ArmorPenetration)); // Goob edit
    }

    private void OnArmorVerbExamine(EntityUid uid, ArmorComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !component.ShowArmorOnExamine)
            return;

        // Shitmed Change Start
        if (component is { ArmourCoverageHidden: true, ArmourModifiersHidden: true })
            return;

        if (!component.Modifiers.Coefficients.Any() && !component.Modifiers.FlatReduction.Any())
            return;

        var examineMarkup = GetArmorExamine(component);
        // Shitmed Change End
        var ev = new ArmorExamineEvent(examineMarkup);
        RaiseLocalEvent(uid, ref ev);

        _examine.AddDetailedExamineVerb(args, component, examineMarkup,
            Loc.GetString("armor-examinable-verb-text"), "/Textures/Interface/VerbIcons/dot.svg.192dpi.png",
            Loc.GetString("armor-examinable-verb-message"));
    }

    // Shitmed Change: Mostly changed.
    private FormattedMessage GetArmorExamine(ArmorComponent component)
    {
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("armor-examine"));

        var coverage = component.ArmorCoverage;
        var armorModifiers = component.Modifiers;

        if (!component.ArmourCoverageHidden)
        {
            // <Trauma>
            var coveredParts = coverage.Where(coveragePart => coveragePart != BodyPartType.Other).ToList();
            List<string> coverageText = [];
            foreach (var part in coveredParts)
                coverageText.Add(Loc.GetString("armor-coverage-type-" + part.ToString().ToLower()));

            msg.PushNewline();
            msg.AddMarkupOrThrow(Loc.GetString("armor-coverage-value", ("type", ContentLocalizationManager.FormatList(coverageText))));
            // </Trauma>
        }

        if (!component.ArmourModifiersHidden)
        {
            foreach (var coefficientArmor in armorModifiers.Coefficients)
            {
                msg.PushNewline();
                var armorType = Loc.GetString("armor-damage-type-" + coefficientArmor.Key.ToLower());
                msg.AddMarkupOrThrow(Loc.GetString("armor-coefficient-value",
                    ("type", armorType),
                    ("value", MathF.Round((1f - coefficientArmor.Value) * 100, 1))
                ));
            }

            foreach (var flatArmor in armorModifiers.FlatReduction)
            {
                msg.PushNewline();

                var armorType = Loc.GetString("armor-damage-type-" + flatArmor.Key.ToLower());
                msg.AddMarkupOrThrow(Loc.GetString("armor-reduction-value",
                    ("type", armorType),
                    ("value", flatArmor.Value)
                ));
            }
        }

        return msg;
    }
}
