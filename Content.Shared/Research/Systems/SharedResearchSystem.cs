// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PoTeletubby <ajcraigaz@gmail.com>
// SPDX-FileCopyrightText: 2024 Rouge2t7 <81053047+Sarahon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Truoizys <153248924+Truoizys@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Lathe;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Research.Systems;

public abstract class SharedResearchSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedLatheSystem _lathe = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TechnologyDatabaseComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, TechnologyDatabaseComponent component, MapInitEvent args)
    {
        UpdateTechnologyCards(uid, component);
    }

    public void UpdateTechnologyCards(EntityUid uid, TechnologyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var availableTechnology = GetAvailableTechnologies(uid, component);
        _random.Shuffle(availableTechnology);

        component.CurrentTechnologyCards.Clear();
        foreach (var discipline in component.SupportedDisciplines)
        {
            var selected = availableTechnology.FirstOrDefault(p => p.Discipline == discipline);
            if (selected == null)
                continue;

            component.CurrentTechnologyCards.Add(selected.ID);
        }
        Dirty(uid, component);
    }

    public List<TechnologyPrototype> GetAvailableTechnologies(EntityUid uid, TechnologyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return new List<TechnologyPrototype>();

        var availableTechnologies = new List<TechnologyPrototype>();
        var disciplineTiers = GetDisciplineTiers(component);
        foreach (var tech in PrototypeManager.EnumeratePrototypes<TechnologyPrototype>())
        {
            if (IsTechnologyAvailable(component, tech, disciplineTiers))
                availableTechnologies.Add(tech);
        }

        return availableTechnologies;
    }

    public bool IsTechnologyAvailable(TechnologyDatabaseComponent component, TechnologyPrototype tech, Dictionary<string, int>? disciplineTiers = null)
    {
        disciplineTiers ??= GetDisciplineTiers(component);

        if (tech.Hidden)
            return false;

        if (!component.SupportedDisciplines.Contains(tech.Discipline))
            return false;

        // if (tech.Tier > disciplineTiers[tech.Discipline])    // Goobstation R&D Console rework - removed main discipline checks
        //     return false;

        if (component.UnlockedTechnologies.Contains(tech.ID))
            return false;

        foreach (var prereq in tech.TechnologyPrerequisites)
        {
            if (!component.UnlockedTechnologies.Contains(prereq))
                return false;
        }

        return true;
    }

    public Dictionary<string, int> GetDisciplineTiers(TechnologyDatabaseComponent component)
    {
        var tiers = new Dictionary<string, int>();
        foreach (var discipline in component.SupportedDisciplines)
        {
            tiers.Add(discipline, GetHighestDisciplineTier(component, discipline));
        }

        return tiers;
    }

    public int GetHighestDisciplineTier(TechnologyDatabaseComponent component, string disciplineId)
    {
        return GetHighestDisciplineTier(component, PrototypeManager.Index<TechDisciplinePrototype>(disciplineId));
    }

    public int GetHighestDisciplineTier(TechnologyDatabaseComponent component, TechDisciplinePrototype techDiscipline)
    {
        var allTech = PrototypeManager.EnumeratePrototypes<TechnologyPrototype>()
            .Where(p => p.Discipline == techDiscipline.ID && !p.Hidden).ToList();
        var allUnlocked = new List<TechnologyPrototype>();
        foreach (var recipe in component.UnlockedTechnologies)
        {
            var proto = PrototypeManager.Index<TechnologyPrototype>(recipe);
            if (proto.Discipline != techDiscipline.ID)
                continue;
            allUnlocked.Add(proto);
        }

        var highestTier = techDiscipline.TierPrerequisites.Keys.Max();
        var tier = 2; //tier 1 is always given

        // todo this might break if you have hidden technologies. i'm not sure

        while (tier <= highestTier)
        {
            // we need to get the tech for the tier 1 below because that's
            // what the percentage in TierPrerequisites is referring to.
            var unlockedTierTech = allUnlocked.Where(p => p.Tier == tier - 1).ToList();
            var allTierTech = allTech.Where(p => p.Discipline == techDiscipline.ID && p.Tier == tier - 1).ToList();

            if (allTierTech.Count == 0)
                break;

            var percent = (float) unlockedTierTech.Count / allTierTech.Count;
            if (percent < techDiscipline.TierPrerequisites[tier])
                break;

            if (tier >= techDiscipline.LockoutTier &&
                component.MainDiscipline != null &&
                techDiscipline.ID != component.MainDiscipline)
                break;
            tier++;
        }

        return tier - 1;
    }

    public FormattedMessage GetTechnologyDescription(
        TechnologyPrototype technology,
        bool includeCost = true,
        bool includeTier = true,
        bool includePrereqs = false,
        TechDisciplinePrototype? disciplinePrototype = null)
    {
        var description = new FormattedMessage();
        if (includeTier)
        {
            disciplinePrototype ??= PrototypeManager.Index(technology.Discipline);
            description.AddMarkupOrThrow(Loc.GetString("research-console-tier-discipline-info",
                ("tier", technology.Tier), ("color", disciplinePrototype.Color), ("discipline", Loc.GetString(disciplinePrototype.Name))));
            description.PushNewline();
        }

        if (includeCost)
        {
            description.AddMarkupOrThrow(Loc.GetString("research-console-cost", ("amount", technology.Cost)));
            description.PushNewline();
        }

        if (includePrereqs && technology.TechnologyPrerequisites.Any())
        {
            description.AddMarkupOrThrow(Loc.GetString("research-console-prereqs-list-start"));
            foreach (var recipe in technology.TechnologyPrerequisites)
            {
                var techProto = PrototypeManager.Index(recipe);
                description.PushNewline();
                description.AddMarkupOrThrow(Loc.GetString("research-console-prereqs-list-entry",
                    ("text", Loc.GetString(techProto.Name))));
            }
            description.PushNewline();
        }

        description.AddMarkupOrThrow(Loc.GetString("research-console-unlocks-list-start"));
        foreach (var recipe in technology.RecipeUnlocks)
        {
            var recipeProto = PrototypeManager.Index(recipe);
            description.PushNewline();
            description.AddMarkupOrThrow(Loc.GetString("research-console-unlocks-list-entry",
                ("name", _lathe.GetRecipeName(recipeProto))));
        }
        foreach (var generic in technology.GenericUnlocks)
        {
            description.PushNewline();
            description.AddMarkupOrThrow(Loc.GetString("research-console-unlocks-list-entry-generic",
                ("text", Loc.GetString(generic.UnlockDescription))));
        }

        return description;
    }

    /// <summary>
    ///     Returns whether a technology is unlocked on this database or not.
    /// </summary>
    /// <returns>Whether it is unlocked or not</returns>
    public bool IsTechnologyUnlocked(EntityUid uid, TechnologyPrototype technology, TechnologyDatabaseComponent? component = null)
    {
        return Resolve(uid, ref component) && IsTechnologyUnlocked(uid, technology.ID, component);
    }

    /// <summary>
    ///     Returns whether a technology is unlocked on this database or not.
    /// </summary>
    /// <returns>Whether it is unlocked or not</returns>
    public bool IsTechnologyUnlocked(EntityUid uid, string technologyId, TechnologyDatabaseComponent? component = null)
    {
        return Resolve(uid, ref component, false) && component.UnlockedTechnologies.Contains(technologyId);
    }

    public void TrySetMainDiscipline(TechnologyPrototype prototype, EntityUid uid, TechnologyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var discipline = PrototypeManager.Index(prototype.Discipline);
        if (prototype.Tier < discipline.LockoutTier)
            return;
        component.MainDiscipline = prototype.Discipline;
        Dirty(uid, component);

        var ev = new TechnologyDatabaseModifiedEvent();
        RaiseLocalEvent(uid, ref ev);
    }

    /// <summary>
    /// Removes a technology and its recipes from a technology database.
    /// </summary>
    public bool TryRemoveTechnology(Entity<TechnologyDatabaseComponent> entity, ProtoId<TechnologyPrototype> tech)
    {
        return TryRemoveTechnology(entity, PrototypeManager.Index(tech));
    }

    /// <summary>
    /// Removes a technology and its recipes from a technology database.
    /// </summary>
    [PublicAPI]
    public bool TryRemoveTechnology(Entity<TechnologyDatabaseComponent> entity, TechnologyPrototype tech)
    {
        if (!entity.Comp.UnlockedTechnologies.Remove(tech.ID))
            return false;

        // check to make sure we didn't somehow get the recipe from another tech.
        // unlikely, but whatever
        var recipes = tech.RecipeUnlocks;
        foreach (var recipe in recipes)
        {
            var hasTechElsewhere = false;
            foreach (var unlockedTech in entity.Comp.UnlockedTechnologies)
            {
                var unlockedTechProto = PrototypeManager.Index<TechnologyPrototype>(unlockedTech);

                if (!unlockedTechProto.RecipeUnlocks.Contains(recipe))
                    continue;
                hasTechElsewhere = true;
                break;
            }

            if (!hasTechElsewhere)
                entity.Comp.UnlockedRecipes.Remove(recipe);
        }
        Dirty(entity, entity.Comp);
        UpdateTechnologyCards(entity, entity);
        return true;
    }

    /// <summary>
    /// Clear all unlocked technologies from the database.
    /// </summary>
    [PublicAPI]
    public void ClearTechs(EntityUid uid, TechnologyDatabaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp) || comp.UnlockedTechnologies.Count == 0)
            return;

        comp.UnlockedTechnologies.Clear();
        Dirty(uid, comp);
    }

    /// <summary>
    /// Adds a lathe recipe to the specified technology database
    /// without checking if it can be unlocked.
    /// </summary>
    public void AddLatheRecipe(EntityUid uid, string recipe, TechnologyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.UnlockedRecipes.Contains(recipe))
            return;

        component.UnlockedRecipes.Add(recipe);
        Dirty(uid, component);

        var ev = new TechnologyDatabaseModifiedEvent();
        RaiseLocalEvent(uid, ref ev);
    }
}