// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Access;
using Content.Goobstation.Common.Charges;
using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Common.Interaction;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Lavaland.Audio;
using Content.Shared.Access.Components;
using Content.Shared.Actions;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Body.Organ;
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
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract class SharedCyberdeckSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;
    [Dependency] protected readonly SharedChargesSystem Charges = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedBossMusicSystem _bossMusic = default!;
    [Dependency] private readonly SharedCryostorageSystem _cryostorage = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    [Dependency] private readonly INetManager _net = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<ContainerManagerComponent> _containerQuery;
    private EntityQuery<LimitedChargesComponent> _chargesQuery;
    private EntityQuery<CyberdeckHackableComponent> _hackQuery;
    private EntityQuery<CyberdeckUserComponent> _userQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberdeckUserComponent, ComponentStartup>(OnUserInit);
        SubscribeLocalEvent<CyberdeckUserComponent, ComponentShutdown>(OnUserShutdown);
        SubscribeLocalEvent<CyberdeckProjectionComponent, GetVerbsEvent<AlternativeVerb>>(OnProjectionVerbs);

        SubscribeLocalEvent<CyberdeckSourceComponent, ChargesChangedEvent>(OnChargesChanged);

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckHackActionEvent>(OnStartHacking);
        SubscribeLocalEvent<CyberdeckHackableComponent, CyberdeckHackDoAfterEvent>(OnHacked);
        SubscribeLocalEvent<AccessReaderComponent, CyberdeckHackDeviceEvent>(OnAccessHacked);
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiLightEvent>(OnLightAiHacked, before: new []{typeof(SharedStationAiSystem)});
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiBoltEvent>(OnAirlockBolt, before: new []{typeof(SharedStationAiSystem)});
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiEmergencyAccessEvent>(OnAirlockEmergencyAccess, before: new []{typeof(SharedStationAiSystem)});
        SubscribeLocalEvent<CyberdeckHackableComponent, StationAiElectrifiedEvent>(OnElectrified, before: new []{typeof(SharedStationAiSystem)});

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionEvent>(OnCyberVisionUsed);
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionReturnEvent>(OnCyberVisionReturn);

        SubscribeLocalEvent<SiliconComponent, BeforeCyberdeckHackPlayerEvent>(BeforeSiliconHacked);

        _handsQuery = GetEntityQuery<HandsComponent>();
        _containerQuery = GetEntityQuery<ContainerManagerComponent>();
        _chargesQuery = GetEntityQuery<LimitedChargesComponent>();
        _hackQuery = GetEntityQuery<CyberdeckHackableComponent>();
        _userQuery = GetEntityQuery<CyberdeckUserComponent>();
    }

    #region Basic User Handling

    private void OnUserInit(Entity<CyberdeckUserComponent> ent, ref ComponentStartup args)
    {
        var (uid, component) = ent;

        _actions.AddAction(uid, ref component.HackAction, component.HackActionId);
        _actions.AddAction(uid, ref component.VisionAction, component.VisionActionId);

        if (!_body.TryGetBodyOrganEntityComps<CyberdeckSourceComponent>(uid, out var organs)
            || organs.Count == 0)
            return;

        component.ProviderEntity = organs[0].Owner;
        UpdateAlert((uid, component));
    }

    private void OnUserShutdown(Entity<CyberdeckUserComponent> ent, ref ComponentShutdown args)
    {
        var (uid, component) = ent;

        UpdateAlert(ent, true);
        DetachFromProjection(ent);

        _actions.RemoveAction(uid, component.HackAction);
        _actions.RemoveAction(uid, component.VisionAction);
        _actions.RemoveAction(uid, component.ReturnAction);

        PredictedQueueDel(ent.Comp.ProjectionEntity);
    }

    private void OnProjectionVerbs(Entity<CyberdeckProjectionComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!HasComp<StationAiHeldComponent>(user))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("cyberdeck-station-ai-smite-verb"),
            Act = () =>
            {
                if (!_userQuery.TryComp(ent.Comp.RemoteEntity, out var userComp))
                    return;

                var sound = ent.Comp.CounterHackSound;
                DetachFromProjection((ent.Comp.RemoteEntity.Value, userComp));

                Popup.PopupClient("cyberdeck-player-get-hacked", ent.Comp.RemoteEntity.Value, ent.Comp.RemoteEntity, PopupType.LargeCaution);
                _audio.PlayLocal(sound, ent.Owner, ent.Owner);
                _audio.PlayLocal(sound, user, user);
            },
            Impact = LogImpact.High,
        });
    }

    #endregion

    #region Charges Handling

    private bool TryHackDevice(EntityUid user, EntityUid device)
    {
        if (!_hackQuery.TryComp(device, out var hackable)
            || !_power.IsPowered(device))
            return false;

        return UseCharges(user, hackable.Cost);
    }

    /// <summary>
    /// Checks and then uses some cyberdeck charges. If cyberdeck provider entity is null,
    /// will just ignore charges and always return true.
    /// </summary>
    private bool UseCharges(EntityUid user, int amount, EntityUid? target = null)
    {
        if (!_userQuery.TryComp(user, out var cyberDeck))
            return false;

        if (cyberDeck.ProviderEntity == null)
            return true; // We don't care if nowhere to take charges from at this point

        if (!CheckCharges(user, cyberDeck.ProviderEntity.Value, amount, target))
            return false;

        Charges.TryUseCharges(cyberDeck.ProviderEntity.Value, amount);
        return true;
    }

    /// <summary>
    /// Checks if the user has enough charges to use ability or hack something
    /// using cyberdeck charges, also handles all related popups.
    /// </summary>
    private bool CheckCharges(EntityUid user, EntityUid provider, int amount, EntityUid? target = null)
    {
        // If we don't have a provider, we also return true so it will give infinite charges (feature)
        if (!_chargesQuery.TryComp(provider, out var chargesComp)
            || Charges.HasCharges((provider, chargesComp), amount))
            return true;

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

    private void OnChargesChanged(Entity<CyberdeckSourceComponent> ent, ref ChargesChangedEvent args)
    {
        if (!TryComp(ent.Owner, out OrganComponent? organ)
            || !_userQuery.TryComp(organ.Body, out var userComp))
            return;

        var user = organ.Body.Value;
        ent.Comp.Accumulator = 0f;
        UpdateAlert((user, userComp));
    }

    #endregion

    #region Hacking Handling

    private void OnStartHacking(Entity<CyberdeckUserComponent> ent, ref CyberdeckHackActionEvent args)
    {
        var (uid, component) = ent;

        if (args.Handled
            || args.Target == uid)
            return;

        args.Handled = true;

        EntityUid? target = null;

        // Starting with most specific cases, moving to most common ones for code safety
        // Prioritize containers over hands, because we want to be able to hack IPCs and borgs
        if (_containerQuery.TryComp(args.Target, out var containerComp))
        {
            // If it's a container, find anything hackable and hack it.
            // No, I won't stack loops inside an if statement, because birds will start migrating to such Nested code.
            foreach (var container in _container.GetAllContainers(args.Target, containerComp))
            {
                var containerTarget = container.ContainedEntities.FirstOrNull(_hackQuery.HasComp);
                if (containerTarget == null)
                    continue;

                target = containerTarget.Value;
                break;
            }
        }

        if (_handsQuery.TryComp(args.Target, out var handsComp)
            && target == null)
        {
            // Check all hands for something that can be hacked
            foreach (var item in _hands.EnumerateHeld(args.Target, handsComp))
            {
                if (!_hackQuery.HasComp(item))
                    continue;

                target = item;
                break;
            }
        }

        if (_hackQuery.HasComp(args.Target))
            target = args.Target;

        // To be safe we get the component itself only here.
        if (!_hackQuery.TryComp(target, out var hackable))
            return;

        if (_hands.GetActiveItem(uid) != null)
        {
            Popup.PopupClient(Loc.GetString("cyberdeck-needs-free-hand"), uid, uid);
            return;
        }

        // Make a popup and return if not enough charges
        if (component.ProviderEntity != null
            && !CheckCharges(uid, component.ProviderEntity.Value, hackable.Cost, target))
            return;

        // Balancing it via ref event that prevents you from hacking IPC batteries in 2 seconds.
        var beforeEv = new BeforeCyberdeckHackPlayerEvent();
        RaiseLocalEvent(args.Target, ref beforeEv);
        var penaltyTime = beforeEv.PenaltyTime;

        var ev = new DoAfterArgs(
            EntityManager,
            uid,
            hackable.HackingTime + penaltyTime,
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

        if (!_doAfter.TryStartDoAfter(ev))
            return;

        var message = Loc.GetString("cyberdeck-start-hacking", ("target", Identity.Entity(target.Value, EntityManager, uid)));
        Popup.PopupClient(message, uid, uid);
        _audio.PlayLocal(component.UserHackingSound, uid, uid);

        // Also alert the target if it's a player (or player targeted a silicon).
        // They can't do anything about it. They will just look at this message and cry.
        if (HasComp<ActorComponent>(target))
        {
            _audio.PlayGlobal(component.VictimHackedSound, target.Value);
            Popup.PopupEntity(Loc.GetString("cyberdeck-player-get-hacked"),
                target.Value,
                target.Value,
                PopupType.LargeCaution);
        }
        if (HasComp<ActorComponent>(args.Target) && HasComp<SiliconComponent>(args.Target))
        {
            _audio.PlayGlobal(component.VictimHackedSound, args.Target);
            Popup.PopupEntity(Loc.GetString("cyberdeck-player-get-hacked"),
                target.Value,
                target.Value,
                PopupType.LargeCaution);
        }
    }

    private void OnHacked(Entity<CyberdeckHackableComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled
            || ent.Owner != args.Target
            || !TryHackDevice(args.User, ent.Owner))
            return;

        // This evil hacking events chain is required to handle charges properly if target has multiple components.
        // For example, hacking an Airlock will open it AND add IgnoreAccess, but it will take charges only once.
        var ev = new CyberdeckHackDeviceEvent(args.User);
        RaiseLocalEvent(ent.Owner, ref ev);

        // Oops. Compensate charges if we failed
        if (ev.Refund)
        {
            if (!_userQuery.TryComp(args.User, out var userComp)
                || userComp.ProviderEntity == null)
                return;

            Charges.AddCharges(userComp.ProviderEntity.Value, ent.Comp.Cost);
        }
        else
        {
            // Spawn hacking effect entity that can be seen by the station AI
            var pos = Transform(ent).Coordinates;
            if (ent.Comp.AfterHackingEffect != null)
                PredictedSpawnAtPosition(ent.Comp.AfterHackingEffect, pos);
        }
    }

    private void OnAccessHacked(Entity<AccessReaderComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        var ignore = EnsureComp<IgnoreAccessComponent>(ent);
        ignore.Ignore.Add(args.User);
    }

    private void HandleAiHacking<T>(EntityUid target, ref T args) where T : BaseStationAiAction
    {
        if (_userQuery.HasComp(args.User))
            args.Cancelled = !TryHackDevice(args.User, target);
    }

    private void OnAirlockBolt(EntityUid ent, CyberdeckHackableComponent component, StationAiBoltEvent args)
        => HandleAiHacking(ent, ref args);

    private void OnAirlockEmergencyAccess(EntityUid ent, CyberdeckHackableComponent component, StationAiEmergencyAccessEvent args)
        => HandleAiHacking(ent, ref args);

    private void OnElectrified(EntityUid ent, CyberdeckHackableComponent component, StationAiElectrifiedEvent args)
        => HandleAiHacking(ent, ref args);

    private void OnLightAiHacked(EntityUid ent, CyberdeckHackableComponent component, StationAiLightEvent args)
        => HandleAiHacking(ent, ref args);

    #endregion

    #region Projection Handling

    /// <summary>
    /// Attaches a player to projection if it already exists,
    /// otherwise creates it and does the same but on server side.
    /// </summary>
    /// <param name="user"></param>
    private void AttachToProjection(Entity<CyberdeckUserComponent> user)
    {
        if (user.Comp.InProjection)
            return;

        // At first, we just add visuals & actions, because they're easily predicted
        EnsureComp<StationAiOverlayComponent>(user.Owner);
        EnsureComp<CyberdeckOverlayComponent>(user.Owner);
        EnsureComp<NoNormalInteractionComponent>(user.Owner);

        _actions.AddAction(user.Owner, ref user.Comp.ReturnAction, user.Comp.ReturnActionId);
        _actions.RemoveAction(user.Owner, user.Comp.VisionAction);
        user.Comp.VisionAction = null; // Shitcode to prevent errors

        _audio.PlayLocal(user.Comp.DiveStartSound, user.Owner, user.Owner);
        _bossMusic.StartBossMusic(user.Comp.DiveMusicId, user.Owner); // Ambient loop

        // Now everything becomes tricky.
        // To make everything work smoothly enough, we need to store the projection entity somewhere.
        // That means that there are 3 possible scenarios:
        // 1. Projection entity is already stored on a paused map, and we know that it exist
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
        if (TerminatingOrDeleted(user.Comp.ProjectionEntity)
            && _net.IsClient)
        {
            Log.Warning($"Cyberdeck Projection was invalid on client-side for user {ToPrettyString(user.Owner)}," +
                        $" and at the same time it's not null, which shouldn't normally happen. This can cause problems with Cyberdeck prediction.");
            return;
        }

        // If it's deleted on a server, then something is really messed up...
        if (TerminatingOrDeleted(user.Comp.ProjectionEntity)
            && _net.IsServer)
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
    private void DetachFromProjection(Entity<CyberdeckUserComponent> user)
    {
        if (user.Comp.ProjectionEntity == null
            || !user.Comp.InProjection)
            return;

        RemComp<StationAiOverlayComponent>(user);
        RemComp<CyberdeckOverlayComponent>(user);
        RemComp<NoNormalInteractionComponent>(user);

        _actions.AddAction(user, ref user.Comp.VisionAction, user.Comp.VisionActionId);
        _actions.RemoveAction(user, user.Comp.ReturnAction);
        user.Comp.ReturnAction = null; // Shitcode to prevent errors

        _audio.PlayLocal(user.Comp.DiveExitSound, user.Owner, user.Owner);
        _bossMusic.EndAllMusic(user.Owner);

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
        // So I'll just steal an already existing paused map instead of shitspamming with a new one.
        // TODO: Make a universal paused map to store things on
        _cryostorage.EnsurePausedMap();
        if (_cryostorage.PausedMap == null)
        {
            Log.Error("CryoSleep map was unexpectedly null");
            return;
        }

        Xform.SetParent(projection, _cryostorage.PausedMap.Value);
        Dirty(user.Owner, user.Comp);
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

    #endregion

    #region Other

    private void BeforeSiliconHacked(Entity<SiliconComponent> ent, ref BeforeCyberdeckHackPlayerEvent args)
        => args.PenaltyTime += ent.Comp.CyberdeckPenaltyTime;

    #endregion

    /// <summary>
    /// Updates an alert, counting how many charges player currently has.
    /// </summary>
    /// <param name="ent">A user to apply the alert.</param>
    /// <param name="doClear">If true, will just remove the alert entirely, until it gets updated again.</param>
    protected virtual void UpdateAlert(Entity<CyberdeckUserComponent> ent, bool doClear = false) { }
}
