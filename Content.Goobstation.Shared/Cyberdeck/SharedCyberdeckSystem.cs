// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Access;
using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Common.Interaction;
using Content.Shared.Access.Components;
using Content.Shared.Actions;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Body.Systems;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract class SharedCyberdeckSystem : EntitySystem
{
    [Dependency] protected readonly ISharedPlayerManager PlayerMan = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedCryostorageSystem _cryostorage = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    protected EntityQuery<HandsComponent> HandsQuery;
    protected EntityQuery<ContainerManagerComponent> ContainerQuery;
    protected EntityQuery<LimitedChargesComponent> ChargesQuery;

    protected EntityQuery<CyberdeckHackableComponent> HackQuery;
    protected EntityQuery<CyberdeckUserComponent> UserQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberdeckHackableComponent, CyberdeckHackDoAfterEvent>(OnHacked);

        SubscribeLocalEvent<CyberdeckProjectionComponent, GetVerbsEvent<Verb>>(OnProjectionVerbs);

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckHackActionEvent>(OnStartHacking);
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionEvent>(OnCyberVisionUsed);
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionReturnEvent>(OnCyberVisionReturn);
        SubscribeLocalEvent<CyberdeckUserComponent, ComponentStartup>(OnUserInit);
        SubscribeLocalEvent<CyberdeckUserComponent, ComponentShutdown>(OnUserShutdown);

        SubscribeLocalEvent<AccessReaderComponent, CyberdeckHackDeviceEvent>(OnAccessHacked);

        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiLightEvent>(OnLightAiHacked, before: new []{typeof(SharedStationAiSystem)});
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiBoltEvent>(OnAirlockBolt, before: new []{typeof(SharedStationAiSystem)});
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiEmergencyAccessEvent>(OnAirlockEmergencyAccess, before: new []{typeof(SharedStationAiSystem)});
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiElectrifiedEvent>(OnElectrified, before: new []{typeof(SharedStationAiSystem)});

        HandsQuery = GetEntityQuery<HandsComponent>();
        ContainerQuery = GetEntityQuery<ContainerManagerComponent>();
        ChargesQuery = GetEntityQuery<LimitedChargesComponent>();

        HackQuery = GetEntityQuery<CyberdeckHackableComponent>();
        UserQuery = GetEntityQuery<CyberdeckUserComponent>();
    }

    private bool TryHackDevice(EntityUid user, EntityUid device)
    {
        if (!HackQuery.TryComp(device, out var hackable)
            || !_power.IsPowered(device))
            return false;

        return UseCharges(user, hackable.Cost);
    }

    private bool UseCharges(EntityUid user, int amount, EntityUid? target = null)
    {
        if (!UserQuery.TryComp(user, out var cyberDeck))
            return false;

        if (cyberDeck.ProviderEntity == null)
            return true; // We don't care if nowhere to take charges from at this point

        if (!CheckCharges(user, cyberDeck.ProviderEntity.Value, amount, target))
            return false;

        _charges.TryUseCharges(cyberDeck.ProviderEntity.Value, amount);
        return true;
    }

    private bool CheckCharges(EntityUid user, EntityUid provider, int amount, EntityUid? target = null)
    {
        if (!ChargesQuery.TryComp(provider, out var chargesComp))
            return true; // Provider doesn't have charges, so we shouldn't care about them

        if (_charges.TryUseCharges(provider, amount))
            return true; // Everything is alright

        // Tell user that he doesn't have enough charges
        string message;
        var charges = chargesComp.LastCharges;
        var chargesForm = amount - charges;

        // SHUT UP C# I HATE BRACES!!!!!!!!!
        // ReSharper disable once EnforceIfStatementBraces
        if (target != null)
            message = Loc.GetString("cyberdeck-insufficient-charges-with-target",
                ("amount", chargesForm),
                ("target", Identity.Entity(target.Value, EntityManager, user)));
        // ReSharper disable once EnforceIfStatementBraces
        else
            message = Loc.GetString("cyberdeck-insufficient-charges",
                ("amount", chargesForm));

        Popup.PopupClient(message, user, user, PopupType.Medium);
        return false;
    }

    private void OnProjectionVerbs(Entity<CyberdeckProjectionComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!HasComp<StationAiHeldComponent>(args.User))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("cyberdeck-station-ai-smite-verb"),
            Act = () => { DetachFromProjection(ent.Comp.RemoteEntity); },
            Impact = LogImpact.High,
        });
    }

    private void OnStartHacking(EntityUid uid, CyberdeckUserComponent component, CyberdeckHackActionEvent args)
    {
        if (args.Handled || args.Target == uid || !_timing.IsFirstTimePredicted)
            return;

        args.Handled = true;
        EntityUid? target = null;

        // Starting with most specific cases, moving to most common ones for code safety
        // Prioritize containers over hands, because we want to be able to hack IPCs and borgs
        if (ContainerQuery.TryComp(args.Target, out var containerComp))
        {
            // If it's a container, find anything hackable and hack it.
            // No, I won't stack loops inside an if statement, because birds will start migrating to such Nested code.
            foreach (var container in _container.GetAllContainers(args.Target, containerComp))
            {
                var containerTarget = container.ContainedEntities.FirstOrNull(HackQuery.HasComp);
                if (containerTarget == null)
                    continue;

                target = containerTarget.Value;
            }
        }

        if (HandsQuery.TryComp(args.Target, out var handsComp) && target == null)
        {
            // Check all hands for something that can be hacked
            foreach (var item in _hands.EnumerateHeld(args.Target, handsComp))
            {
                if (!HackQuery.HasComp(item))
                    continue;

                target = item;
                break;
            }
        }

        if (HackQuery.HasComp(args.Target))
            target = args.Target;

        // To be safe we get the component itself only here.
        if (!HackQuery.TryComp(target, out var hackable))
            return;

        // Make a popup and return if not enough charges
        if (component.ProviderEntity != null
            && !CheckCharges(uid, component.ProviderEntity.Value, hackable.Cost, target))
            return;

        var ev = new DoAfterArgs(
            EntityManager,
            uid,
            hackable.HackingTime,
            new CyberdeckHackDoAfterEvent(),
            target,
            target,
            component.ProviderEntity,
            uid)
        {
            BlockDuplicate = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            DistanceThreshold = 20f,
            Broadcast = false,
            Hidden = true,
            RequireCanInteract = false,
            ColorOverride = Color.Aquamarine,
        };

        _doAfter.TryStartDoAfter(ev);

        if (_net.IsClient)
            return; // Im too lazy to fix popups.

        var message = Loc.GetString("cyberdeck-start-hacking", ("target", Identity.Entity(target.Value, EntityManager, uid)));
        Popup.PopupEntity(message, uid, uid);

        // Also alert the target if it's a player.
        // They can't do anything about it. They will just look at this message and cry.
        if (HasComp<ActorComponent>(target))
            Popup.PopupEntity(Loc.GetString("cyberdeck-player-get-hacked"), target.Value, target.Value, PopupType.LargeCaution);
    }

    private void OnUserInit(Entity<CyberdeckUserComponent> ent, ref ComponentStartup args)
    {
        var (uid, component) = ent;

        _actions.AddAction(uid, ref component.HackAction, component.HackActionId);
        _actions.AddAction(uid, ref component.VisionAction, component.VisionActionId);
        UpdateAlert((uid, component));

        // Find the cyberdeck source by hand. TODO: Maybe make a BodyOrganRelayEvent and subscribe to it?
        var evil = _body.GetBodyOrgans(uid).Where(x => HasComp<CyberdeckSourceComponent>(x.Id)).FirstOrNull();
        if (evil == null)
            return;

        component.ProviderEntity = evil.Value.Id;
    }

    private void OnUserShutdown(Entity<CyberdeckUserComponent> ent, ref ComponentShutdown args)
    {
        var (uid, component) = ent;

        _actions.RemoveAction(uid, component.HackAction);
        _actions.RemoveAction(uid, component.VisionAction);
        _actions.RemoveAction(uid, component.ReturnAction);

        UpdateAlert(ent, true);

        DetachFromProjection(ent);

        // We don't need a projection anymore as component related to it is deleted.
        PredictedQueueDel(ent.Comp.ProjectionEntity);
    }

    private void OnHacked(Entity<CyberdeckHackableComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!TryHackDevice(args.User, ent.Owner))
            return;

        // This evil hacking events chain is required to handle charges properly if target has multiple components.
        // For example, hacking an Airlock will open it AND add IgnoreAccess, but it will take charges only once.
        var ev = new CyberdeckHackDeviceEvent(args.User);
        RaiseLocalEvent(ent.Owner, ref ev);

        // Oops. Compensate charges if we failed
        if (ev.Refund)
        {
            if (!UserQuery.TryComp(args.User, out var userComp)
                || userComp.ProviderEntity == null)
                return;

            _charges.AddCharges(userComp.ProviderEntity.Value, ent.Comp.Cost);
        }
    }

    private void OnAccessHacked(Entity<AccessReaderComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        var ignore = EnsureComp<IgnoreAccessComponent>(ent);
        ignore.Ignore.Add(args.User);
    }

    private void OnAirlockBolt(EntityUid ent, CyberdeckHackableComponent component, StationAiBoltEvent args)
    {
        if (UserQuery.HasComp(args.User))
            args.Cancelled = !TryHackDevice(args.User, ent);
    }

    private void OnAirlockEmergencyAccess(EntityUid ent, CyberdeckHackableComponent component, StationAiEmergencyAccessEvent args)
    {
        if (UserQuery.HasComp(args.User))
            args.Cancelled = !TryHackDevice(args.User, ent);
    }

    private void OnElectrified(EntityUid ent, CyberdeckHackableComponent component, StationAiElectrifiedEvent args)
    {
        if (UserQuery.HasComp(args.User))
            args.Cancelled = !TryHackDevice(args.User, ent);
    }

    private void OnLightAiHacked(EntityUid ent, CyberdeckHackableComponent component, StationAiLightEvent args)
    {
        if (UserQuery.HasComp(args.User))
            args.Cancelled = !TryHackDevice(args.User, ent);
    }

    private void OnCyberVisionUsed(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        if (!UseCharges(uid, comp.CyberVisionAbilityCost))
            return;

        AttachToProjection(ent);
        args.Handled = true;
    }

    private void OnCyberVisionReturn(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionReturnEvent args)
    {
        if (args.Handled)
            return;

        DetachFromProjection(ent);
        args.Handled = true;
    }

    /// <summary>
    /// Attaches a player to projection if it already exists,
    /// otherwise creates it and does the same but on server side.
    /// </summary>
    /// <param name="user"></param>
    private void AttachToProjection(Entity<CyberdeckUserComponent> user)
    {
        if (user.Comp.InProjection)
            return;

        // At first we just add visuals & actions, because they're easily predicted
        EnsureComp<StationAiOverlayComponent>(user.Owner);
        EnsureComp<CyberdeckOverlayComponent>(user.Owner);
        EnsureComp<NoNormalInteractionComponent>(user.Owner);

        _actions.AddAction(user.Owner, ref user.Comp.ReturnAction, user.Comp.ReturnActionId);
        _actions.RemoveAction(user.Owner, user.Comp.VisionAction);

        // Now everything becomes tricky.
        // At this point there are 3 possible scenarios:
        // 1. Projection entity is already stored in nullspace, and we know that it exist
        // 2. Same as 1 but we for some reason think that it's deleted
        // 3. Projection entity doesn't exist

        // Only in the first case we can actually do something.

        // Handle case 3 (projection doesn't exist)
        if (user.Comp.ProjectionEntity == null)
        {
            if (_net.IsClient)
                return;

            var newProjection = Spawn(user.Comp.ProjectionEntityId, MapCoordinates.Nullspace);
            SetPaused(newProjection, true);
            var projectionComp = EnsureComp<CyberdeckProjectionComponent>(newProjection);

            projectionComp.RemoteEntity = user.Owner;
            user.Comp.ProjectionEntity = newProjection;

            Dirty(user.Owner, user.Comp);
            Dirty(newProjection, projectionComp);
        }

        // Client thinks that projection is not null, but is deleted.
        // This normally shouldn't happen, but if it is what it is then we need to cope with what we have.
        if (TerminatingOrDeleted(user.Comp.ProjectionEntity) && _net.IsClient)
        {
            Log.Warning($"Cyberdeck Projection was invalid on client-side for user {ToPrettyString(user.Owner)}," +
                        $" and at the same time it's not null, which shouldn't normally happen. This can cause problems with Cyberdeck prediction.");
            return;
        }

        // If it's deleted on a server, then something is really messed up...
        if (TerminatingOrDeleted(user.Comp.ProjectionEntity) && _net.IsServer)
        {
            Log.Error($"Failed to create Cyberdeck projection for user {ToPrettyString(user.Owner)}, or it was deleted incorrectly at some time!");
            return;
        }

        // Handle the standard case, when we just need to pull an existing entity from Nullspace
        var projection = user.Comp.ProjectionEntity!.Value; // If it's null then we're cooked already
        var position = Transform(user).Coordinates;

        SetPaused(projection, false);
        Xform.SetCoordinates(projection, position);

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, false, eyeComp);
            _eye.SetTarget(user, projection, eyeComp);
        }

        _mover.SetRelay(user, projection);
        user.Comp.InProjection = true;
    }

    /// <summary>
    /// Detaches player from a projection forcefully, and sends an existing projection to Nullspace.
    /// </summary>
    protected void DetachFromProjection(Entity<CyberdeckUserComponent> user)
    {
        if (user.Comp.ProjectionEntity == null || !user.Comp.InProjection)
            return;

        RemComp<StationAiOverlayComponent>(user);
        RemComp<CyberdeckOverlayComponent>(user);
        RemComp<NoNormalInteractionComponent>(user);

        _actions.AddAction(user, ref user.Comp.VisionAction, user.Comp.VisionActionId);
        _actions.RemoveAction(user, user.Comp.ReturnAction);

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, true, eyeComp);
            _eye.SetTarget(user, null, eyeComp);
        }

        RemComp<RelayInputMoverComponent>(user);

        // We did everything to put the player back in place,
        // now let's try to save the projection for smoother prediction
        user.Comp.InProjection = false;

        if (TerminatingOrDeleted(user.Comp.ProjectionEntity))
            return;

        var projection = user.Comp.ProjectionEntity.Value;

        // This probably is a dirty solution, but surprisingly you can't send entities to Nullspace...
        // So i'll just steal an already existing pause map instead of shitspamming with a new one.
        _cryostorage.EnsurePausedMap();
        if (_cryostorage.PausedMap == null)
        {
            Log.Error("CryoSleep map was unexpectedly null");
            return;
        }

        Xform.SetParent(projection, _cryostorage.PausedMap.Value);
        Dirty(user.Owner, user.Comp);
    }

    /// <summary>
    /// Detaches player from a projection forcefully, and sends an existing projection to Nullspace.
    /// This specific overload lets you put in a nullable EntityUid.
    /// </summary>
    private void DetachFromProjection(Entity<CyberdeckUserComponent?>? user)
    {
        if (user == null)
            return;

        var userEnt = user.Value;

        if (!Resolve(userEnt.Owner, ref userEnt.Comp))
            return;

        DetachFromProjection((userEnt.Owner, userEnt.Comp));
    }

    /// <summary>
    /// Updates an alert, counting how many charges player currently has.
    /// </summary>
    /// <param name="ent">A user to apply the alert.</param>
    /// <param name="doClear">If true, will just remove the alert entirely, until it gets updated again.</param>
    protected virtual void UpdateAlert(Entity<CyberdeckUserComponent> ent, bool doClear = false) { }
}
