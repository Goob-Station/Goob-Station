// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Wrexbe (Josh) <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 vanx <61917534+Vaaankas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Goobstation.Wizard.ArcaneBarrage;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Localizations;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem : EntitySystem
{
    private void InitializeInteractions()
    {
        SubscribeAllEvent<RequestSetHandEvent>(HandleSetHand);
        SubscribeAllEvent<RequestActivateInHandEvent>(HandleActivateItemInHand);
        SubscribeAllEvent<RequestHandInteractUsingEvent>(HandleInteractUsingInHand);
        SubscribeAllEvent<RequestUseInHandEvent>(HandleUseInHand);
        SubscribeAllEvent<RequestMoveHandItemEvent>(HandleMoveItemFromHand);
        SubscribeAllEvent<RequestHandAltInteractEvent>(HandleHandAltInteract);

        SubscribeLocalEvent<HandsComponent, GetUsedEntityEvent>(OnGetUsedEntity);
        SubscribeLocalEvent<HandsComponent, ExaminedEvent>(HandleExamined);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.UseItemInHand, InputCmdHandler.FromDelegate(HandleUseItem, handle: false, outsidePrediction: false))
            .Bind(ContentKeyFunctions.AltUseItemInHand, InputCmdHandler.FromDelegate(HandleAltUseInHand, handle: false, outsidePrediction: false))
            .Bind(ContentKeyFunctions.SwapHands, InputCmdHandler.FromDelegate(SwapHandsPressed, handle: false, outsidePrediction: false))
            .Bind(ContentKeyFunctions.Drop, new PointerInputCmdHandler(DropPressed))
            .Register<SharedHandsSystem>();
    }

    #region Event and Key-binding Handlers
    private void HandleAltUseInHand(ICommonSession? session)
    {
        if (session?.AttachedEntity != null)
            TryUseItemInHand(session.AttachedEntity.Value, true);
    }

    private void HandleUseItem(ICommonSession? session)
    {
        if (session?.AttachedEntity != null)
            TryUseItemInHand(session.AttachedEntity.Value);
    }

    private void HandleMoveItemFromHand(RequestMoveHandItemEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity != null)
            TryMoveHeldEntityToActiveHand(args.SenderSession.AttachedEntity.Value, msg.HandName);
    }

    private void HandleUseInHand(RequestUseInHandEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity != null)
            TryUseItemInHand(args.SenderSession.AttachedEntity.Value);
    }

    private void HandleActivateItemInHand(RequestActivateInHandEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity != null)
            TryActivateItemInHand(args.SenderSession.AttachedEntity.Value, null, msg.HandName);
    }

    private void HandleInteractUsingInHand(RequestHandInteractUsingEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity != null)
            TryInteractHandWithActiveHand(args.SenderSession.AttachedEntity.Value, msg.HandName);
    }

    private void HandleHandAltInteract(RequestHandAltInteractEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity != null)
            TryUseItemInHand(args.SenderSession.AttachedEntity.Value, true, handName: msg.HandName);
    }

    private void SwapHandsPressed(ICommonSession? session)
    {
        if (!TryComp(session?.AttachedEntity, out HandsComponent? component))
            return;

        if (!_actionBlocker.CanInteract(session.AttachedEntity.Value, null))
            return;

        if (component.ActiveHand == null || component.Hands.Count < 2)
            return;

        var newActiveIndex = component.SortedHands.IndexOf(component.ActiveHand.Name) + 1;
        var nextHand = component.SortedHands[newActiveIndex % component.Hands.Count];

        TrySetActiveHand(session.AttachedEntity.Value, nextHand, component);
    }

    private bool DropPressed(ICommonSession? session, EntityCoordinates coords, EntityUid netEntity)
    {
        if (TryComp(session?.AttachedEntity, out HandsComponent? hands) && hands.ActiveHand != null)
        {
            // Goobstation start
            if (_net.IsServer && HasComp<DeleteOnDropAttemptComponent>(hands.ActiveHandEntity))
            {
                QueueDel(hands.ActiveHandEntity.Value);
                return false;
            }

            if (session != null)
            {
                var ent = session.AttachedEntity.Value;

                if (TryGetActiveItem(ent, out var item) && TryComp<VirtualItemComponent>(item, out var virtComp))
                {
                    var userEv = new VirtualItemDropAttemptEvent(virtComp.BlockingEntity, ent, item.Value, false);
                    RaiseLocalEvent(ent, userEv);

                    var targEv = new VirtualItemDropAttemptEvent(virtComp.BlockingEntity, ent, item.Value, false);
                    RaiseLocalEvent(virtComp.BlockingEntity, targEv);

                    if (userEv.Cancelled || targEv.Cancelled)
                        return false;
                }
                TryDrop(ent, hands.ActiveHand, coords, handsComp: hands);
            }
            // Goobstation end
        }

        // always send to server.
        return false;
    }
    #endregion

    public bool TryActivateItemInHand(EntityUid uid, HandsComponent? handsComp = null, string? handName = null)
    {
        if (!Resolve(uid, ref handsComp, false))
            return false;

        Hand? hand;
        if (handName == null || !handsComp.Hands.TryGetValue(handName, out hand))
            hand = handsComp.ActiveHand;

        if (hand?.HeldEntity is not { } held)
            return false;

        return _interactionSystem.InteractionActivate(uid, held);
    }

    public bool TryInteractHandWithActiveHand(EntityUid uid, string handName, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp, false))
            return false;

        if (handsComp.ActiveHandEntity == null)
            return false;

        if (!handsComp.Hands.TryGetValue(handName, out var hand))
            return false;

        if (hand.HeldEntity == null)
            return false;

        _interactionSystem.InteractUsing(uid, handsComp.ActiveHandEntity.Value, hand.HeldEntity.Value, Transform(hand.HeldEntity.Value).Coordinates);
        return true;
    }

    public bool TryUseItemInHand(EntityUid uid, bool altInteract = false, HandsComponent? handsComp = null, string? handName = null)
    {
        if (!Resolve(uid, ref handsComp, false))
            return false;

        Hand? hand;
        if (handName == null || !handsComp.Hands.TryGetValue(handName, out hand))
            hand = handsComp.ActiveHand;

        if (hand?.HeldEntity is not { } held)
            return false;

        if (altInteract)
            return _interactionSystem.AltInteract(uid, held);
        else
            return _interactionSystem.UseInHandInteraction(uid, held);
    }

    /// <summary>
    ///     Moves an entity from one hand to the active hand.
    /// </summary>
    public bool TryMoveHeldEntityToActiveHand(EntityUid uid, string handName, bool checkActionBlocker = true, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp))
            return false;

        if (handsComp.ActiveHand == null || !handsComp.ActiveHand.IsEmpty)
            return false;

        if (!handsComp.Hands.TryGetValue(handName, out var hand))
            return false;

        if (!CanDropHeld(uid, hand, checkActionBlocker))
            return false;

        var entity = hand.HeldEntity!.Value;

        if (!CanPickupToHand(uid, entity, handsComp.ActiveHand, checkActionBlocker, handsComp))
            return false;

        DoDrop(uid, hand, false, handsComp, log:false);
        DoPickup(uid, handsComp.ActiveHand, entity, handsComp, log: false);
        return true;
    }

    private void OnGetUsedEntity(EntityUid uid, HandsComponent component, ref GetUsedEntityEvent args)
    {
        if (args.Handled)
            return;

        if (component.ActiveHandEntity.HasValue)
        {
            // allow for the item to return a different entity, e.g. virtual items
            RaiseLocalEvent(component.ActiveHandEntity.Value, ref args);
        }

        args.Used ??= component.ActiveHandEntity;
    }

    //TODO: Actually shows all items/clothing/etc.
    private void HandleExamined(EntityUid examinedUid, HandsComponent handsComp, ExaminedEvent args)
    {
        var heldItemNames = EnumerateHeld(examinedUid, handsComp)
            .Where(entity => !HasComp<VirtualItemComponent>(entity))
            .Select(item => FormattedMessage.EscapeText(Identity.Name(item, EntityManager)))
            .Select(itemName => Loc.GetString("comp-hands-examine-wrapper", ("item", itemName)))
            .ToList();

        var locKey = heldItemNames.Count != 0 ? "comp-hands-examine" : "comp-hands-examine-empty";
        var locUser = ("user", Identity.Entity(examinedUid, EntityManager));
        var locItems = ("items", ContentLocalizationManager.FormatList(heldItemNames));

        // WWDP examine
        if (args.Examiner == args.Examined) // Use the selfaware locale when inspecting yourself
            locKey += "-selfaware";

        using (args.PushGroup(nameof(HandsComponent), 99)) //  priority for examine
        {
            args.PushMarkup("- " + Loc.GetString(locKey, locUser, locItems)); // "-" for better formatting
        }
        // WWDP edit end
    }
}