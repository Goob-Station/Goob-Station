// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared.Eye;
using Content.Shared.Ghost;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mind;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems;

public abstract class ScryingOrbSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedGhostSystem _ghost = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    private static readonly EntProtoId ObserverProto = "MobObserverWizard";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScryingOrbComponent, GetVerbsEvent<InteractionVerb>>(OnGetInteractionVerb);
        SubscribeLocalEvent<ScryingOrbComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<ScryingOrbComponent, GotEquippedHandEvent>(OnEquipHand);
        SubscribeLocalEvent<ScryingOrbComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<ScryingOrbComponent, GotUnequippedHandEvent>(OnUnequipHand);
        SubscribeLocalEvent<ScryingOrbComponent, GotUnequippedEvent>(OnUnequip);
    }

    public bool IsEquipped(EntityUid uid)
    {
        var scryingOrbQuery = GetEntityQuery<ScryingOrbComponent>();
        if (_hands.EnumerateHeld(uid).Any(scryingOrbQuery.HasComponent))
            return true;

        var enumerator = _inventory.GetSlotEnumerator(uid);
        while (enumerator.MoveNext(out var container))
        {
            if (scryingOrbQuery.HasComp(container.ContainedEntity))
                return true;
        }

        return false;
    }

    private void OnEquip(Entity<ScryingOrbComponent> ent, ref GotEquippedEvent args)
    {
        if (!TryComp(args.Equipee, out EyeComponent? eye))
            return;

        _eye.SetVisibilityMask(args.Equipee, eye.VisibilityMask | (int) VisibilityFlags.Ghost, eye);
    }

    private void OnUnequip(Entity<ScryingOrbComponent> ent, ref GotUnequippedEvent args)
    {
        AttemptDisableXRay(args.Equipee);
    }

    private void OnUnequipHand(Entity<ScryingOrbComponent> ent, ref GotUnequippedHandEvent args)
    {
        AttemptDisableXRay(args.User);
    }

    private void OnEquipHand(Entity<ScryingOrbComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!TryComp(args.User, out EyeComponent? eye))
            return;

        _eye.SetVisibilityMask(args.User, eye.VisibilityMask | (int) VisibilityFlags.Ghost, eye);
    }

    private void AttemptDisableXRay(EntityUid uid)
    {
        if (!TryComp(uid, out EyeComponent? eye))
            return;

        if (IsEquipped(uid))
            return;

        _eye.SetVisibilityMask(uid, eye.VisibilityMask & (int) ~VisibilityFlags.Ghost, eye);
        _eye.SetDrawFov(uid, true, eye);
        _eye.SetDrawLight((uid, eye), true);
    }

    private void OnActivate(Entity<ScryingOrbComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || HasComp<GhostComponent>(args.User))
            return;

        Ghost(args.User);
    }

    private void OnGetInteractionVerb(Entity<ScryingOrbComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || HasComp<GhostComponent>(args.User))
            return;

        var user = args.User;
        args.Verbs.Add(new()
        {
            Act = () =>
            {
                Ghost(user);
            },
            Message = Loc.GetString("scrying-orb-verb-message"),
            Text = Loc.GetString("scrying-orb-verb-text"),
        });
    }

    private void Ghost(EntityUid user)
    {
        if (!_mind.TryGetMind(user, out var mind, out var mindComp))
            return;

        var ghost = Spawn(ObserverProto, Transform(user).Coordinates);
        _xform.AttachToGridOrMap(ghost);
        _playerManager.TryGetSessionById(mindComp.UserId, out var session);

        if (!string.IsNullOrWhiteSpace(mindComp.CharacterName))
            _meta.SetEntityName(ghost, mindComp.CharacterName);
        else if (!string.IsNullOrWhiteSpace(session?.Name))
            _meta.SetEntityName(ghost, session.Name);

        if (!TryComp(user, out GhostComponent? ghostComp))
            return;

        _mind.Visit(mind, ghost, mindComp);
        _ghost.SetCanReturnToBody((user, ghostComp), true);
    }
}
