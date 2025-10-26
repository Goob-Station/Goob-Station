// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Stack;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Nutrition;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Map; // Goobstation
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Stacks;

// Partial for general system code and event handlers.
/// <summary>
/// System for handling entities which represent a stack of identical items, usually materials.
/// </summary>
[UsedImplicitly]
public abstract partial class SharedStackSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IViewVariablesManager _vvm = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedHandsSystem Hands = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!; // Goobstation - Custom stack splitting dialog

    // TODO: These should be in the prototype.
    public static readonly int[] DefaultSplitAmounts = { 1, 5, 10, 20, 30, 50 };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StackComponent, InteractUsingEvent>(OnStackInteractUsing);
        SubscribeLocalEvent<StackComponent, ComponentGetState>(OnStackGetState);
        SubscribeLocalEvent<StackComponent, ComponentHandleState>(OnStackHandleState);
        SubscribeLocalEvent<StackComponent, ComponentStartup>(OnStackStarted);
        SubscribeLocalEvent<StackComponent, ExaminedEvent>(OnStackExamined);

        SubscribeLocalEvent<StackComponent, BeforeIngestedEvent>(OnBeforeEaten);
        SubscribeLocalEvent<StackComponent, IngestedEvent>(OnEaten);
        SubscribeLocalEvent<StackComponent, GetVerbsEvent<AlternativeVerb>>(OnStackAlternativeInteract);

        _vvm.GetTypeHandler<StackComponent>()
            .AddPath(nameof(StackComponent.Count), (_, comp) => comp.Count, SetCount);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _vvm.GetTypeHandler<StackComponent>()
            .RemovePath(nameof(StackComponent.Count));
    }

    private void OnStackInteractUsing(Entity<StackComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<StackComponent>(args.Used, out var recipientStack))
            return;

        // Transfer stacks from ground to hand
        if (!TryMergeStacks((ent.Owner, ent.Comp), (args.Used, recipientStack), out var transferred))
            return; // if nothing transferred, leave without a pop-up

        args.Handled = true;

        // interaction is done, the rest is just generating a pop-up

        var popupPos = args.ClickLocation;
        var userCoords = Transform(args.User).Coordinates;

        if (!popupPos.IsValid(EntityManager))
        {
            popupPos = userCoords;
        }

        switch (transferred)
        {
            case > 0:
                Popup.PopupClient($"+{transferred}", popupPos, args.User);

                if (GetAvailableSpace(recipientStack) == 0)
                {
                    Popup.PopupClient(Loc.GetString("comp-stack-becomes-full"),
                        popupPos.Offset(new Vector2(0, -0.5f)),
                        args.User);
                }

                break;

            case 0 when GetAvailableSpace(recipientStack) == 0:
                Popup.PopupClient(Loc.GetString("comp-stack-already-full"), popupPos, args.User);
                break;
        }

        var localRotation = Transform(args.Used).LocalRotation;
        _storage.PlayPickupAnimation(args.Used, popupPos, userCoords, localRotation, args.User);
    }

    private void OnStackStarted(Entity<StackComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp(ent.Owner, out AppearanceComponent? appearance))
            return;

        Appearance.SetData(ent.Owner, StackVisuals.Actual, ent.Comp.Count, appearance);
        Appearance.SetData(ent.Owner, StackVisuals.MaxCount, GetMaxCount(ent.Comp), appearance);
        Appearance.SetData(ent.Owner, StackVisuals.Hide, false, appearance);
    }

    /// <summary>
    /// Goobstation - virtual method to allow calling from shared.
    /// Does nothing on the client.
    /// </summary>
    public virtual EntityUid? Split(EntityUid uid, int amount, EntityCoordinates spawnPosition, StackComponent? stack = null)
    {
        return null;
    }

    private void OnStackGetState(Entity<StackComponent> ent, ref ComponentGetState args)
    {
        args.State = new StackComponentState(ent.Comp.Count, ent.Comp.MaxCountOverride, ent.Comp.Unlimited);
    }

    private void OnStackHandleState(Entity<StackComponent> ent, ref ComponentHandleState args)
    {
        if (args.Current is not StackComponentState cast)
            return;

        ent.Comp.MaxCountOverride = cast.MaxCountOverride;
        ent.Comp.Unlimited = cast.Unlimited;
        // This will change the count and call events.
        SetCount(ent.AsNullable(), cast.Count);
    }

    private void OnStackExamined(Entity<StackComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(
            Loc.GetString("comp-stack-examine-detail-count",
                ("count", ent.Comp.Count),
                ("markupCountColor", "lightgray")
            )
        );
    }

    private void OnBeforeEaten(Entity<StackComponent> eaten, ref BeforeIngestedEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Solution is not { } sol)
            return;

        // If the entity is empty and is a lingering entity we can't eat from it.
        if (eaten.Comp.Count <= 0)
        {
            args.Cancelled = true;
            return;
        }

        // If we've made it this far, we should refresh the solution when this item is eaten provided it's not the last one in the stack!
        args.Refresh = eaten.Comp.Count > 1;

        /*
        Edible stacked items is near completely evil so we must choose one of the following:
        - Option 1: Eat the entire solution each bite and reduce the stack by 1.
        - Option 2: Multiply the solution eaten by the stack size.
        - Option 3: Divide the solution consumed by stack size.
        The easiest and safest option is and always will be Option 1 otherwise we risk reagent deletion or duplication.
        That is why we cancel if we cannot set the minimum to the entire volume of the solution.
        */
        if (args.TryNewMinimum(sol.Volume))
            return;

        args.Cancelled = true;
    }

    private void OnEaten(Entity<StackComponent> eaten, ref IngestedEvent args)
    {
        ReduceCount(eaten.AsNullable(), 1);
    }

    private void OnStackAlternativeInteract(Entity<StackComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || ent.Comp.Count == 1)
            return;

        var user = args.User; // Can't pass ref events into verbs

        AlternativeVerb halve = new()
        {
            Text = Loc.GetString("comp-stack-split-halve"),
            Category = VerbCategory.Split,
            Act = () => UserSplit(ent, user, ent.Comp.Count / 2),
            Priority = 1
        };
        args.Verbs.Add(halve);

        var priority = 0;
        foreach (var amount in DefaultSplitAmounts)
        {
            if (amount >= ent.Comp.Count)
                continue;

            AlternativeVerb verb = new()
            {
                Text = amount.ToString(),
                Category = VerbCategory.Split,
                Act = () => UserSplit(ent, user, amount),
                // we want to sort by size, not alphabetically by the verb text.
                Priority = priority
            };

            priority--;

            args.Verbs.Add(verb);
        }
        // Goobstation - Custom stack splitting dialog
        AlternativeVerb custom = new()
        {
            Text = Loc.GetString("comp-stack-split-custom"),
            Category = VerbCategory.Split,
            Act = () =>
            {
                _ui.OpenUi(user, StackCustomSplitUiKey.Key, user);
            },
            Priority = priority - 1
        };
        args.Verbs.Add(custom);
    }

    /// <remarks>
    ///     OnStackAlternativeInteract() was moved to shared in order to faciliate prediction of stack splitting verbs.
    ///     However, prediction of interacitons with spawned entities is non-functional (or so i'm told)
    ///     So, UserSplit() and Split() should remain on the server for the time being.
    ///     This empty virtual method allows for UserSplit() to be called on the server from the client.
    ///     When prediction is improved, those two methods should be moved to shared, in order to predict the splitting itself (not just the verbs)
    /// </remarks>
    public virtual void UserSplit(Entity<StackComponent> stack, Entity<TransformComponent?> user, int amount) // Goob public
    {

    }
}

/// <summary>
/// Event raised when a stack's count has changed.
/// </summary>
public sealed class StackCountChangedEvent : EntityEventArgs
{
    /// <summary>
    /// The old stack count.
    /// </summary>
    public int OldCount;

    /// <summary>
    /// The new stack count.
    /// </summary>
    public int NewCount;

    public StackCountChangedEvent(int oldCount, int newCount)
    {
        OldCount = oldCount;
        NewCount = newCount;
    }
}
