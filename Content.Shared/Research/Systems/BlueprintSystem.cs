// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Lathe;
using Content.Shared.Popups;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared.Research.Systems;

public sealed class BlueprintSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BlueprintReceiverComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlueprintReceiverComponent, AfterInteractUsingEvent>(OnAfterInteract);
        SubscribeLocalEvent<BlueprintReceiverComponent, LatheGetRecipesEvent>(OnGetRecipes);
    }

    private void OnStartup(Entity<BlueprintReceiverComponent> ent, ref ComponentStartup args)
    {
        _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
    }

    private void OnAfterInteract(Entity<BlueprintReceiverComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach || !TryComp<BlueprintComponent>(args.Used, out var blueprintComponent))
            return;
        args.Handled = TryInsertBlueprint(ent, (args.Used, blueprintComponent), args.User);
    }

    private void OnGetRecipes(Entity<BlueprintReceiverComponent> ent, ref LatheGetRecipesEvent args)
    {
        var recipes = GetBlueprintRecipes(ent);
        foreach (var recipe in recipes)
        {
            args.Recipes.Add(recipe);
        }
    }

    public bool TryInsertBlueprint(Entity<BlueprintReceiverComponent> ent, Entity<BlueprintComponent> blueprint, EntityUid? user)
    {
        if (!CanInsertBlueprint(ent, blueprint, user))
            return false;

        if (user is not null)
        {
            var userId = Identity.Entity(user.Value, EntityManager);
            var bpId = Identity.Entity(blueprint, EntityManager);
            var machineId = Identity.Entity(ent, EntityManager);
            var msg = Loc.GetString("blueprint-receiver-popup-insert",
                ("user", userId),
                ("blueprint", bpId),
                ("receiver", machineId));
            _popup.PopupPredicted(msg, ent, user);
        }

        _container.Insert(blueprint.Owner, _container.GetContainer(ent, ent.Comp.ContainerId));

        var ev = new TechnologyDatabaseModifiedEvent();
        RaiseLocalEvent(ent, ref ev);
        return true;
    }

    public bool CanInsertBlueprint(Entity<BlueprintReceiverComponent> ent, Entity<BlueprintComponent> blueprint, EntityUid? user)
    {
        if (_entityWhitelist.IsWhitelistFail(ent.Comp.Whitelist, blueprint))
        {
            return false;
        }

        if (blueprint.Comp.ProvidedRecipes.Count == 0)
        {
            Log.Error($"Attempted to insert blueprint {ToPrettyString(blueprint)} with no recipes.");
            return false;
        }

        // Don't add new blueprints if there are no new recipes.
        var currentRecipes = GetBlueprintRecipes(ent);
        if (currentRecipes.Count != 0 && currentRecipes.IsSupersetOf(blueprint.Comp.ProvidedRecipes))
        {
            _popup.PopupPredicted(Loc.GetString("blueprint-receiver-popup-recipe-exists"), ent, user);
            return false;
        }

        return _container.CanInsert(blueprint, _container.GetContainer(ent, ent.Comp.ContainerId));
    }

    public HashSet<ProtoId<LatheRecipePrototype>> GetBlueprintRecipes(Entity<BlueprintReceiverComponent> ent)
    {
        var contained = _container.GetContainer(ent, ent.Comp.ContainerId);

        var recipes = new HashSet<ProtoId<LatheRecipePrototype>>();
        foreach (var blueprint in contained.ContainedEntities)
        {
            if (!TryComp<BlueprintComponent>(blueprint, out var blueprintComponent))
                continue;

            foreach (var provided in blueprintComponent.ProvidedRecipes)
            {
                recipes.Add(provided);
            }
        }

        return recipes;
    }
}