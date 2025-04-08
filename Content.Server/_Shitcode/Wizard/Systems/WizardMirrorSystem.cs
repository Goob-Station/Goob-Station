using Content.Server.Guardian;
using Content.Server.Humanoid;
using Content.Server.Mind;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.MagicMirror;
using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.IdentityManagement;
using Content.Shared.Polymorph;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class WizardMirrorSystem : SharedWizardMirrorSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly GrammarSystem _grammar = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<WizardMirrorComponent>(WizardMirrorUiKey.Key,
            subs =>
            {
                subs.Event<BoundUIClosedEvent>(OnUiClosed);
                subs.Event<WizardMirrorMessage>(OnMessage);
            });
    }

    private void OnMessage(Entity<WizardMirrorComponent> ent, ref WizardMirrorMessage args)
    {
        if (!TryComp(ent.Comp.Target, out HumanoidAppearanceComponent? humanoid))
            return;

        ForceLoadProfile(ent.Comp.Target.Value, ent.Comp, args.Profile, humanoid);
    }

    private void OnUiClosed(Entity<WizardMirrorComponent> ent, ref BoundUIClosedEvent args)
    {
        ent.Comp.Target = null;
        Dirty(ent);
    }

    private void ForceLoadProfile(EntityUid target,
        WizardMirrorComponent component,
        HumanoidCharacterProfile profile,
        HumanoidAppearanceComponent humanoid)
    {
        var age = humanoid.Age;
        if (humanoid.Species != profile.Species && component.AllowedSpecies.Contains(profile.Species) &&
            _proto.TryIndex(profile.Species, out var speciesProto))
        {
            if (HasComp<GuardianHostComponent>(target))
            {
                _popup.PopupEntity(Loc.GetString("wizard-mirror-guardian-change-species-fail"), target, target);
                return;
            }

            var config = new PolymorphConfiguration
            {
                Entity = speciesProto.Prototype,
                TransferName = true,
                TransferDamage = true,
                Forced = true,
                Inventory = PolymorphInventoryChange.Transfer,
                RevertOnCrit = false,
                RevertOnDeath = false,
                ComponentsToTransfer = new()
                {
                    new("Wizard", mirror: true),
                    new ("Apprentice", mirror: true),
                    new ("NpcFactionMember"),
                },
            };
            var newUid = _polymorph.PolymorphEntity(target, config);
            if (newUid != null)
            {
                RemCompDeferred<PolymorphedEntityComponent>(newUid.Value);
                humanoid = EnsureComp<HumanoidAppearanceComponent>(newUid.Value);
                target = newUid.Value;
                _humanoid.SetSpecies(target, profile.Species, false, humanoid);
            }
        }

        _meta.SetEntityName(target, profile.Name);
        _humanoid.SetSex(target, profile.Sex, false, humanoid);
        humanoid.EyeColor = profile.Appearance.EyeColor;

        _humanoid.SetSkinColor(target, profile.Appearance.SkinColor, false, false);

        humanoid.MarkingSet.Clear();

        // Add markings that doesn't need coloring. We store them until we add all other markings that doesn't need it.
        var markingFColored = new Dictionary<Marking, MarkingPrototype>();
        foreach (var marking in profile.Appearance.Markings)
        {
            if (!_markingManager.TryGetMarking(marking, out var prototype))
                continue;

            if (!prototype.ForcedColoring)
            {
                _humanoid.AddMarking(target, marking.MarkingId, marking.MarkingColors, false);
            }
            else
            {
                markingFColored.Add(marking, prototype);
            }
        }

        // Don't limit hair color but still apply the restrictions to them
        var hairColor = profile.Appearance.HairColor;
        var facialHairColor = profile.Appearance.FacialHairColor;

        if (_markingManager.Markings.TryGetValue(profile.Appearance.HairStyleId, out var hairPrototype) &&
            _markingManager.CanBeApplied(profile.Species, profile.Sex, hairPrototype, _proto))
        {
            _humanoid.AddMarking(target, profile.Appearance.HairStyleId, hairColor, false);
        }

        if (_markingManager.Markings.TryGetValue(profile.Appearance.FacialHairStyleId, out var facialHairPrototype) &&
            _markingManager.CanBeApplied(profile.Species, profile.Sex, facialHairPrototype, _proto))
        {
            _humanoid.AddMarking(target, profile.Appearance.FacialHairStyleId, facialHairColor, false);
        }

        // Finally adding marking with forced colors
        foreach (var (marking, prototype) in markingFColored)
        {
            var markingColors = MarkingColoring.GetMarkingLayerColors(
                prototype,
                profile.Appearance.SkinColor,
                profile.Appearance.EyeColor,
                humanoid.MarkingSet
            );
            _humanoid.AddMarking(target, marking.MarkingId, markingColors, false);
        }

        humanoid.MarkingSet.EnsureSpecies(profile.Species, profile.Appearance.SkinColor, _markingManager, _proto);

        humanoid.MarkingSet.EnsureDefault(humanoid.SkinColor, humanoid.EyeColor, _markingManager);

        humanoid.Gender = profile.Gender;
        if (TryComp<GrammarComponent>(target, out var grammar))
            _grammar.SetGender((target, grammar), profile.Gender);
        var identity = Identity.Entity(target, EntityManager);
        if (TryComp(identity, out GrammarComponent? identityGrammar))
            _grammar.SetGender((identity, identityGrammar), profile.Gender);

        humanoid.Age = age;

        if (_mind.TryGetMind(target, out var mind, out _) && TryComp(mind, out SoulBoundComponent? soulBound))
        {
            soulBound.Name = profile.Name;
            soulBound.Age = age;
            soulBound.Gender = profile.Gender;
            soulBound.Sex = profile.Sex;
            Dirty(mind, soulBound);
        }

        RaiseLocalEvent(target, new ProfileLoadFinishedEvent());
        Dirty(target, humanoid);
    }
}
