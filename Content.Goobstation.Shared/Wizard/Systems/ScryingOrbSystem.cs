// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Wizard.Components;
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

public sealed partial class ScryingOrbSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedGhostSystem _ghost = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    private EntityQuery<EyeComponent> _eyeQuery;
    private EntityQuery<GhostComponent> _ghostQuery;
    private EntityQuery<ScryingOrbComponent> _scryingOrbQuery;

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

        _eyeQuery = EntityManager.GetEntityQuery<EyeComponent>();
        _ghostQuery = EntityManager.GetEntityQuery<GhostComponent>();
        _scryingOrbQuery = EntityManager.GetEntityQuery<ScryingOrbComponent>();
    }

    private void HandleEquip(EntityUid user)
    {
        EnsureComp<ScryingViewerComponent>(user);

        if (!_eyeQuery.TryComp(user, out var eye))
            return;

        _eye.SetVisibilityMask(user, eye.VisibilityMask | (int) VisibilityFlags.Ghost, eye);
    }

    private void HandleUnequip(EntityUid user)
    {
        // Would still fire Unequipped if its in their hand or moved to another slot probably
        // make sure it Doesnt
        if (IsEquipped(user))
            return;

        RemComp<ScryingViewerComponent>(user);

        if (!_eyeQuery.TryComp(user, out var eye))
            return;

        _eye.SetVisibilityMask(user, eye.VisibilityMask & (int) ~VisibilityFlags.Ghost, eye);
        _eye.SetDrawFov(user, true, eye);
        _eye.SetDrawLight((user, eye), true);
    }

    /// <summary>
    /// Get whether a scrying orb is equipped anywhere on the entity.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool IsEquipped(EntityUid uid)
    {
        if (_hands.EnumerateHeld(uid).Any(_scryingOrbQuery.HasComp))
            return true;

        var enumerator = _inventory.GetSlotEnumerator(uid);
        while (enumerator.MoveNext(out var container))
        {
            if (_scryingOrbQuery.HasComp(container.ContainedEntity))
                return true;
        }

        return false;
    }

    private void OnEquip(Entity<ScryingOrbComponent> ent, ref GotEquippedEvent args)
        => HandleEquip(args.Equipee);

    private void OnEquipHand(Entity<ScryingOrbComponent> ent, ref GotEquippedHandEvent args)
        => HandleEquip(args.User);

    private void OnUnequip(Entity<ScryingOrbComponent> ent, ref GotUnequippedEvent args)
        => HandleUnequip(args.Equipee);

    private void OnUnequipHand(Entity<ScryingOrbComponent> ent, ref GotUnequippedHandEvent args)
        => HandleUnequip(args.User);

    private void OnActivate(Entity<ScryingOrbComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || _ghostQuery.HasComp(args.User))
            return;

        Ghost(args.User);
    }

    private void OnGetInteractionVerb(Entity<ScryingOrbComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || _ghostQuery.HasComp(args.User))
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

        var ghost = PredictedSpawnAtPosition(ObserverProto, Transform(user).Coordinates);
        _xform.AttachToGridOrMap(ghost);
        _playerManager.TryGetSessionById(mindComp.UserId, out var session);

        if (!string.IsNullOrWhiteSpace(mindComp.CharacterName))
            _meta.SetEntityName(ghost, mindComp.CharacterName);
        else if (!string.IsNullOrWhiteSpace(session?.Name))
            _meta.SetEntityName(ghost, session.Name);

        _mind.Visit(mind, ghost, mindComp);
        _ghost.SetCanReturnToBody((ghost, _ghostQuery.GetComponent(ghost)), true);
    }
}
