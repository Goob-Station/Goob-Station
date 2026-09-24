using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.IntrinsicVoiceModulator.Components;
using Content.Goobstation.Shared.VoiceChat;
using Content.Server.Atmos.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Telephone;
using Content.Server.VoiceMask;
using Content.Shared.ActionBlocker;
using Content.Shared.Chat;
using Content.Shared.Clothing.Components;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Holopad;
using Content.Shared.Implants.Components;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Radio.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Shared.StationAi;
using Content.Shared.SurveillanceCamera.Components;
using Content.Shared.Tag;
using Content.Shared.Telephone;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceChatSystem : EntitySystem
{
    [Dependency] private readonly VoiceChatManager _voice = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IServerNetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;
    [Dependency] private readonly TelephoneSystem _telephone = default!;
    [Dependency] private readonly VoiceBroadcastSystem _broadcast = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly VoiceRadioSystem _radioVoice = default!;

    private static readonly TimeSpan TransmissionGap = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan PermissionRecheck = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan RefusalDisplay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan StateRefreshInterval = TimeSpan.FromMilliseconds(500);
    private const float ListenerMargin = 2f;
    private const float WhisperRange = 3f;
    private static readonly ProtoId<TagPrototype> MuzzleTag = "Muzzle";

    private readonly Dictionary<NetUserId, Speaker> _speakers = new();
    private readonly HashSet<NetUserId> _adminMuted = new();
    private readonly Dictionary<NetUserId, Route> _routes = new();
    private readonly List<Relay> _relays = new();
    private readonly HashSet<ICommonSession> _broadcastRecipients = new();
    private readonly List<EntityUid> _radioReceivers = new();
    private readonly HashSet<ICommonSession> _radioRecipients = new();
    private readonly List<Relay> _radioRelays = new();
    private readonly HashSet<Entity<StationAiVisionComponent>> _cameras = new();

    private EntityQuery<GhostComponent> _ghostQuery;
    private EntityQuery<EyeComponent> _eyeQuery;
    private EntityQuery<SpeechComponent> _speechQuery;
    private EntityQuery<StationAiHeldComponent> _aiHeldQuery;
    private EntityQuery<SurveillanceCameraComponent> _cameraQuery;
    private EntityQuery<HolopadComponent> _holopadQuery;
    private ICommonSession[]? _sessions;
    private ushort _nextSpeakerId = 1;
    private float _range;
    private TimeSpan _nextStateRefresh;

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();
        _eyeQuery = GetEntityQuery<EyeComponent>();
        _speechQuery = GetEntityQuery<SpeechComponent>();
        _aiHeldQuery = GetEntityQuery<StationAiHeldComponent>();
        _cameraQuery = GetEntityQuery<SurveillanceCameraComponent>();
        _holopadQuery = GetEntityQuery<HolopadComponent>();

        Subs.CVar(_cfg, GoobCVars.VoiceChatRange, value => _range = value, true);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.VoicePushToTalk,
                InputCmdHandler.FromDelegate(
                    session => SetPushToTalk(session, true),
                    session => SetPushToTalk(session, false),
                    handle: false))
            .Register<VoiceChatSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<VoiceChatSystem>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.RealTime;
        _routes.Clear();
        _sessions = null;

        while (_voice.TryDequeueEvent(out var ev))
        {
            switch (ev)
            {
                case VoiceWebFrame frame:
                    HandleFrame(frame, now);
                    break;
                case VoiceWebConnectionChanged changed:
                    _voice.SendStatus(changed.User);
                    SendState(changed.User, true);
                    UpdateEffect(changed.User);
                    break;
                case VoiceWebEffectSelected selected:
                    GetSpeaker(selected.User).SelectedEffect = selected.Effect;
                    UpdateEffect(selected.User);
                    break;
            }
        }

        if (now < _nextStateRefresh)
            return;

        _nextStateRefresh = now + StateRefreshInterval;
        foreach (var user in _speakers.Keys)
        {
            SendState(user, false);
            UpdateEffect(user);
        }
    }

    public bool ToggleAdminMute(NetUserId user)
    {
        var muted = _adminMuted.Add(user);
        if (!muted)
            _adminMuted.Remove(user);

        SendState(user, false);
        return muted;
    }

    private void HandleFrame(VoiceWebFrame frame, TimeSpan now)
    {
        if (!_player.TryGetSessionById(frame.User, out var session) ||
            session.Status != SessionStatus.InGame ||
            session.AttachedEntity is not { } uid)
        {
            return;
        }

        var speaker = GetSpeaker(frame.User);
        var levels = speaker.Measure(frame.Payload);

        if (_adminMuted.Contains(frame.User))
        {
            SendSelf(session, speaker, levels, VoiceSelfFlags.Blocked);
            return;
        }

        var newTransmission = now - speaker.LastAttempt > TransmissionGap;
        speaker.LastAttempt = now;

        if (!CanTransmit(speaker, uid, now, newTransmission))
        {
            SendSelf(session, speaker, levels, VoiceSelfFlags.Blocked);
            return;
        }

        if (newTransmission)
            speaker.InfoSent.Clear();

        if (!_routes.TryGetValue(frame.User, out var route))
        {
            route = BuildRoute(session, uid);
            _routes[frame.User] = route;
            UpdateEffect(frame.User);
            SendSpeakerInfo(speaker, uid, route, session.Channel);

            foreach (var telephone in route.Telephones)
            {
                _telephone.KeepCallAlive(telephone);
            }
        }

        var selfFlags = VoiceSelfFlags.None;
        if (route.RadioChannel != null)
            selfFlags |= VoiceSelfFlags.Radio;
        if (route.Broadcasting)
            selfFlags |= VoiceSelfFlags.Broadcast;

        SendSelf(session, speaker, levels, selfFlags);

        byte[]? radioPayload = null;
        foreach (var group in route.Groups)
        {
            var payload = frame.Payload;
            if (group.Route is VoiceRoute.Radio or VoiceRoute.RadioSpeaker)
                payload = radioPayload ??= speaker.ApplyRadio(frame.Payload);

            _net.ServerSendToMany(new MsgVoiceFrame
            {
                Source = group.Source,
                Speaker = speaker.Id,
                Sequence = frame.Sequence,
                Flags = frame.Flags,
                Route = group.Route,
                Global = group.Global,
                Range = group.Range,
                Payload = payload,
            }, group.Channels);
        }
    }

    private void SendSelf(ICommonSession session, Speaker speaker, VoiceLevels levels, VoiceSelfFlags flags)
    {
        _net.ServerSendMessage(new MsgVoiceSelf
        {
            Speaker = speaker.Id,
            Flags = flags,
            Levels = levels,
        }, session.Channel);
    }

    private void SendSpeakerInfo(Speaker speaker, EntityUid uid, Route route, INetChannel self)
    {
        var name = GetSpeakerName(uid);
        var channel = route.RadioChannel ?? string.Empty;
        if (name != speaker.InfoName || channel != speaker.InfoChannel)
        {
            speaker.InfoName = name;
            speaker.InfoChannel = channel;
            speaker.InfoSent.Clear();
        }

        MsgVoiceSpeakerInfo? message = null;
        foreach (var group in route.Groups)
        {
            foreach (var recipient in group.Channels)
            {
                if (!speaker.InfoSent.Add(recipient))
                    continue;

                message ??= CreateSpeakerInfo(speaker);
                _net.ServerSendMessage(message, recipient);
            }
        }

        if (speaker.InfoSent.Add(self))
            _net.ServerSendMessage(message ?? CreateSpeakerInfo(speaker), self);
    }

    private static MsgVoiceSpeakerInfo CreateSpeakerInfo(Speaker speaker)
    {
        return new MsgVoiceSpeakerInfo
        {
            Speaker = speaker.Id,
            Name = speaker.InfoName,
            Channel = speaker.InfoChannel,
        };
    }

    private string GetSpeakerName(EntityUid uid)
    {
        var nameEv = new TransformSpeakerNameEvent(uid, Name(uid));
        RaiseLocalEvent(uid, nameEv);

        if (_aiHeldQuery.HasComp(uid) && nameEv.VoiceName == Prototype(uid)?.Name)
            return Loc.GetString("job-name-station-ai");

        return nameEv.VoiceName;
    }

    private Route BuildRoute(ICommonSession speakerSession, EntityUid speakerUid)
    {
        var route = new Route();
        var ghostSpeaker = _ghostQuery.HasComp(speakerUid);

        var direct = GetDirectEmitter(speakerUid);
        var directRoute = _aiHeldQuery.HasComp(speakerUid) ? VoiceRoute.Camera : VoiceRoute.Direct;
        var directOrigin = MapCoordinates.Nullspace;
        if (direct is { } directUid)
        {
            directOrigin = _transform.GetMapCoordinates(directUid);
            if (directOrigin.MapId == MapId.Nullspace)
                direct = null;
        }

        var broadcastSource = NetEntity.Invalid;

        _relays.Clear();
        _broadcastRecipients.Clear();
        _radioRecipients.Clear();
        _radioRelays.Clear();

        if (!ghostSpeaker)
        {
            CollectRelays(speakerUid, route);

            if (_broadcast.TryGetBroadcastConsole(speakerUid, out var console))
            {
                route.Broadcasting = true;
                broadcastSource = GetNetEntity(console);
                _broadcast.GetRecipients(console, _broadcastRecipients);
            }

            route.RadioChannel = CollectRadio(speakerUid);
        }

        _sessions ??= _player.Sessions;
        foreach (var session in _sessions)
        {
            if (session.Status != SessionStatus.InGame ||
                session.AttachedEntity is not { } listener ||
                !_voice.Receives(session.UserId))
            {
                continue;
            }

            if (session == speakerSession && !_voice.HearsSelf(session.UserId))
                continue;

            if (ghostSpeaker && !_ghostQuery.HasComp(listener))
                continue;

            var primary = GetListenerCoordinates(listener);
            MapCoordinates? secondary = _aiHeldQuery.HasComp(listener) ? _transform.GetMapCoordinates(listener) : null;

            if (direct is { } emitter && CanHear(directOrigin, _range, primary, secondary, out var global))
            {
                route.Add(GetNetEntity(emitter), directRoute, global, _range, session.Channel);
            }
            else if (TryHearRelay(_relays, primary, secondary, out var relay, out global))
            {
                route.Add(GetNetEntity(relay.Emitter), relay.Route, global, relay.Range, session.Channel);
            }
            else if (_broadcastRecipients.Contains(session))
            {
                route.Add(broadcastSource, VoiceRoute.Broadcast, true, 0f, session.Channel);
            }
            else if (_radioRecipients.Contains(session) && HearsRadio(session, route))
            {
                route.Add(NetEntity.Invalid, VoiceRoute.Radio, true, 0f, session.Channel);
            }
            else if (HearsRadio(session, route) && TryHearRelay(_radioRelays, primary, secondary, out relay, out global))
            {
                route.Add(GetNetEntity(relay.Emitter), VoiceRoute.RadioSpeaker, global, relay.Range, session.Channel);
            }
        }

        return route;
    }

    private bool HearsRadio(ICommonSession session, Route route)
    {
        return route.RadioChannel is { } channel && !_voice.IsRadioChannelMuted(session.UserId, channel);
    }

    private EntityUid? GetDirectEmitter(EntityUid speaker)
    {
        if (!_aiHeldQuery.HasComp(speaker))
            return speaker;

        if (!_stationAi.TryGetCore(speaker, out var core) ||
            core.Comp is not { Remote: true, RemoteEntity: { } eye })
        {
            return null;
        }

        return FindCamera(eye);
    }

    private EntityUid? FindCamera(EntityUid eye)
    {
        var origin = _transform.GetMapCoordinates(eye);
        if (origin.MapId == MapId.Nullspace)
            return null;

        _cameras.Clear();
        _lookup.GetEntitiesInRange(origin, _range, _cameras);

        EntityUid? best = null;
        var bestDistance = float.MaxValue;
        foreach (var camera in _cameras)
        {
            if (!camera.Comp.Enabled ||
                !_cameraQuery.TryComp(camera, out var surveillance) ||
                !surveillance.Active ||
                camera.Comp.NeedsPower && !_power.IsPowered(camera))
            {
                continue;
            }

            var xform = Transform(camera);
            if (camera.Comp.NeedsAnchoring && !xform.Anchored)
                continue;

            var distance = Vector2.DistanceSquared(_transform.GetWorldPosition(xform), origin.Position);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = camera;
        }

        return best;
    }

    private void CollectRelays(EntityUid speaker, Route route)
    {
        var speakerXform = Transform(speaker);
        var origin = _transform.GetMapCoordinates(speaker, speakerXform);
        if (origin.MapId == MapId.Nullspace)
            return;

        var query = EntityQueryEnumerator<TelephoneComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var telephone, out var xform))
        {
            if (telephone.LinkedTelephones.Count == 0 || telephone.Muted || xform.MapID != origin.MapId)
                continue;

            var entity = new Entity<TelephoneComponent>(uid, telephone);
            if (!_telephone.IsTelephonePowered(entity))
                continue;

            var range = telephone.ListeningRange;
            if (Vector2.DistanceSquared(_transform.GetWorldPosition(xform), origin.Position) > range * range)
                continue;

            if (speakerXform.ParentUid != uid && !_interaction.InRangeUnobstructed(speaker, uid, range))
                continue;

            route.Telephones.Add(entity);

            foreach (var linked in telephone.LinkedTelephones)
            {
                if (linked.Owner == uid || !_telephone.IsTelephonePowered(linked))
                    continue;

                var emitter = GetRelayEmitter(linked);
                var emitterOrigin = _transform.GetMapCoordinates(emitter);
                if (emitterOrigin.MapId == MapId.Nullspace)
                    continue;

                var relayRange = linked.Comp.SpeakerVolume == TelephoneVolume.Speak ? _range : WhisperRange;
                var relayRoute = _holopadQuery.HasComp(linked) ? VoiceRoute.Holopad : VoiceRoute.Telephone;
                _relays.Add(new Relay(emitter, emitterOrigin, relayRange, relayRoute));
                route.Telephones.Add(linked);
            }
        }
    }

    private EntityUid GetRelayEmitter(Entity<TelephoneComponent> telephone)
    {
        if (_holopadQuery.TryComp(telephone, out var holopad) &&
            holopad.Hologram is { } hologram &&
            !TerminatingOrDeleted(hologram))
        {
            return hologram;
        }

        return telephone.Comp.Speaker?.Owner ?? telephone.Owner;
    }

    private string? CollectRadio(EntityUid speaker)
    {
        if (!_radioVoice.TryGetTransmission(speaker, out var channel, out var radioSource))
            return null;

        _radioReceivers.Clear();
        _radioVoice.GetReceivers(radioSource, channel, _radioReceivers);

        foreach (var receiver in _radioReceivers)
        {
            if (TryComp<ActorComponent>(receiver, out var actor))
            {
                _radioRecipients.Add(actor.PlayerSession);
                continue;
            }

            if (TryComp<HeadsetComponent>(receiver, out var headset))
            {
                if (headset.IsEquipped && TryComp<ActorComponent>(Transform(receiver).ParentUid, out var wearer))
                    _radioRecipients.Add(wearer.PlayerSession);
                continue;
            }

            if (TryComp<RadioSpeakerComponent>(receiver, out var radioSpeaker) && radioSpeaker.Enabled)
            {
                var origin = _transform.GetMapCoordinates(receiver);
                if (origin.MapId != MapId.Nullspace)
                    _radioRelays.Add(new Relay(receiver, origin, radioSpeaker.SpeakNormally ? _range : WhisperRange, VoiceRoute.RadioSpeaker));
            }
        }

        return channel.ID;
    }

    private static bool TryHearRelay(List<Relay> relays, MapCoordinates primary, MapCoordinates? secondary, out Relay relay, out bool global)
    {
        relay = default;
        global = false;

        var bestDistance = float.MaxValue;
        foreach (var candidate in relays)
        {
            if (!CanHear(candidate.Origin, candidate.Range, primary, secondary, out var candidateGlobal))
                continue;

            var position = candidateGlobal && secondary is { } fallback ? fallback : primary;
            var distance = Vector2.DistanceSquared(candidate.Origin.Position, position.Position);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            relay = candidate;
            global = candidateGlobal;
        }

        return bestDistance < float.MaxValue;
    }

    private static bool CanHear(MapCoordinates origin, float range, MapCoordinates primary, MapCoordinates? secondary, out bool global)
    {
        var maxDistance = range + ListenerMargin;
        var maxDistanceSquared = maxDistance * maxDistance;

        global = false;
        if (primary.MapId == origin.MapId && Vector2.DistanceSquared(primary.Position, origin.Position) <= maxDistanceSquared)
            return true;

        global = true;
        return secondary is { } other &&
               other.MapId == origin.MapId &&
               Vector2.DistanceSquared(other.Position, origin.Position) <= maxDistanceSquared;
    }

    private void UpdateEffect(NetUserId user)
    {
        var speaker = GetSpeaker(user);
        var settings = default(VoiceEffectSettings);

        if (_player.TryGetSessionById(user, out var session) && session.AttachedEntity is { } uid && !_ghostQuery.HasComp(uid))
        {
            var effect = VoiceEffect.None;
            if (speaker.SelectedEffect != VoiceEffect.None && HasVoiceChanger(uid))
                effect = speaker.SelectedEffect;
            else if (TryComp<VoiceChatEffectComponent>(uid, out var builtIn))
                effect = builtIn.Effect;

            settings = new VoiceEffectSettings(effect, GetMuffle(uid));
        }

        if (settings == speaker.AppliedEffect)
            return;

        speaker.AppliedEffect = settings;
        _voice.SetEffect(user, settings);
    }

    private VoiceMuffle GetMuffle(EntityUid uid)
    {
        if (_container.TryGetContainingContainer((uid, null, null), out var container) &&
            !HasComp<StationAiHolderComponent>(container.Owner) &&
            !_hands.IsHolding(container.Owner, uid))
        {
            return VoiceMuffle.Heavy;
        }

        if (_inventory.TryGetSlotEntity(uid, "mask", out var mask) && CoversMouth(mask.Value))
            return VoiceMuffle.Light;

        if (_inventory.TryGetSlotEntity(uid, "head", out var head) &&
            (HasComp<PressureProtectionComponent>(head) || CoversMouth(head.Value)))
        {
            return VoiceMuffle.Light;
        }

        return VoiceMuffle.None;
    }

    private bool CoversMouth(EntityUid item)
    {
        return TryComp<IngestionBlockerComponent>(item, out var blocker) &&
               blocker.Enabled &&
               !(TryComp<MaskComponent>(item, out var mask) && mask.IsToggled);
    }

    private bool IsMuzzled(EntityUid uid)
    {
        return _inventory.TryGetSlotEntity(uid, "mask", out var mask) &&
               _tag.HasTag(mask.Value, MuzzleTag) &&
               !(TryComp<MaskComponent>(mask, out var toggle) && toggle.IsToggled);
    }

    private bool HasVoiceChanger(EntityUid uid)
    {
        if (HasComp<IntrinsicVoiceModulatorComponent>(uid))
            return true;

        if (_inventory.TryGetContainerSlotEnumerator(uid, out var slots, SlotFlags.WITHOUT_POCKET))
        {
            while (slots.NextItem(out var item))
            {
                if (IsActiveVoiceMask(item))
                    return true;
            }
        }

        if (TryComp<ImplantedComponent>(uid, out var implanted))
        {
            foreach (var implant in implanted.ImplantContainer.ContainedEntities)
            {
                if (IsActiveVoiceMask(implant))
                    return true;
            }
        }

        return false;
    }

    private bool IsActiveVoiceMask(EntityUid uid)
    {
        return TryComp<VoiceMaskComponent>(uid, out var mask) && mask.Active;
    }

    private bool CanTransmit(Speaker speaker, EntityUid uid, TimeSpan now, bool recheck)
    {
        if (!recheck && speaker.CheckedEntity == uid && now - speaker.CheckedAt < PermissionRecheck)
            return speaker.Allowed;

        var allowed = CanSpeak(uid);
        var changed = allowed != speaker.Allowed || speaker.CheckedEntity != uid;

        speaker.CheckedEntity = uid;
        speaker.CheckedAt = now;
        speaker.Allowed = allowed;

        if (changed)
            SendState(speaker.User, false);

        return allowed;
    }

    private bool CanSpeak(EntityUid uid)
    {
        if (_ghostQuery.HasComp(uid))
            return true;

        if (!CanSpeakPassive(uid))
            return false;

        return _actionBlocker.CanSpeak(uid);
    }

    private bool CanSpeakPassive(EntityUid uid)
    {
        if (_ghostQuery.HasComp(uid))
            return true;

        if (_mobState.IsIncapacitated(uid) || HasComp<MutedComponent>(uid) || IsMuzzled(uid))
            return false;

        return _speechQuery.TryComp(uid, out var speech) && speech.Enabled;
    }

    private MapCoordinates GetListenerCoordinates(EntityUid listener)
    {
        if (_eyeQuery.TryComp(listener, out var eye) && eye.Target is { } target && Exists(target))
            return _transform.GetMapCoordinates(target);

        return _transform.GetMapCoordinates(listener);
    }

    private void SetPushToTalk(ICommonSession? session, bool active)
    {
        if (session == null)
            return;

        var speaker = GetSpeaker(session.UserId);
        if (speaker.PushToTalk == active)
            return;

        speaker.PushToTalk = active;
        SendState(session.UserId, false);
    }

    private void SendState(NetUserId user, bool force)
    {
        if (!_voice.IsWebConnected(user))
            return;

        var speaker = GetSpeaker(user);
        var state = BuildState(user, speaker);
        if (!force && speaker.LastState == state)
            return;

        speaker.LastState = state;
        _voice.SendState(user, state);
    }

    private VoiceWebState BuildState(NetUserId user, Speaker speaker)
    {
        if (!_player.TryGetSessionById(user, out var session))
            return new VoiceWebState(string.Empty, false, false, _adminMuted.Contains(user), false, false, false, null);

        if (session.Status != SessionStatus.InGame || session.AttachedEntity is not { } uid)
            return new VoiceWebState(session.Name, false, false, _adminMuted.Contains(user), false, false, false, null);

        var refused = speaker.CheckedEntity == uid &&
                      !speaker.Allowed &&
                      _timing.RealTime - speaker.LastAttempt < RefusalDisplay;

        return new VoiceWebState(
            session.Name,
            true,
            !refused && CanSpeakPassive(uid),
            _adminMuted.Contains(user),
            speaker.PushToTalk,
            _broadcast.TryGetBroadcastConsole(uid, out _),
            HasVoiceChanger(uid),
            _radioVoice.TryGetTransmission(uid, out var radioChannel, out _) ? radioChannel.LocalizedName : null);
    }

    private Speaker GetSpeaker(NetUserId user)
    {
        if (_speakers.TryGetValue(user, out var speaker))
            return speaker;

        speaker = new Speaker(user, _nextSpeakerId);
        _nextSpeakerId = _nextSpeakerId == ushort.MaxValue ? (ushort) 1 : (ushort) (_nextSpeakerId + 1);
        _speakers[user] = speaker;
        return speaker;
    }

    private readonly record struct Relay(EntityUid Emitter, MapCoordinates Origin, float Range, VoiceRoute Route);

    private sealed class Route
    {
        public readonly List<RouteGroup> Groups = new();
        public readonly List<Entity<TelephoneComponent>> Telephones = new();
        public string? RadioChannel;
        public bool Broadcasting;

        public void Add(NetEntity source, VoiceRoute voiceRoute, bool global, float range, INetChannel channel)
        {
            foreach (var group in Groups)
            {
                if (group.Source == source && group.Route == voiceRoute && group.Global == global && group.Range.Equals(range))
                {
                    group.Channels.Add(channel);
                    return;
                }
            }

            var created = new RouteGroup(source, voiceRoute, global, range);
            created.Channels.Add(channel);
            Groups.Add(created);
        }
    }

    private sealed class RouteGroup(NetEntity source, VoiceRoute route, bool global, float range)
    {
        public readonly NetEntity Source = source;
        public readonly VoiceRoute Route = route;
        public readonly bool Global = global;
        public readonly float Range = range;
        public readonly List<INetChannel> Channels = new();
    }

    private sealed class Speaker(NetUserId user, ushort id)
    {
        public readonly NetUserId User = user;
        public readonly ushort Id = id;
        public bool PushToTalk;
        public bool Allowed;
        public EntityUid? CheckedEntity;
        public TimeSpan CheckedAt;
        public TimeSpan LastAttempt;
        public VoiceWebState? LastState;
        public VoiceEffect SelectedEffect;
        public VoiceEffectSettings AppliedEffect;
        public string InfoName = string.Empty;
        public string InfoChannel = string.Empty;
        public readonly HashSet<INetChannel> InfoSent = new();

        private readonly VoiceLevelAnalyzer _levelAnalyzer = new();
        private readonly short[] _levelPcm = new short[VoiceCodec.FrameSamples];
        private readonly VoiceEffectProcessor _radioEffects = new();
        private readonly short[] _radioPcm = new short[VoiceCodec.FrameSamples];
        private int _radioPredictor;
        private int _radioIndex;

        public VoiceLevels Measure(byte[] payload)
        {
            return VoiceCodec.Decode(payload, _levelPcm) ? _levelAnalyzer.Analyze(_levelPcm) : default;
        }

        public byte[] ApplyRadio(byte[] payload)
        {
            if (!VoiceCodec.Decode(payload, _radioPcm))
                return payload;

            _radioEffects.Process(_radioPcm, 0, _radioPcm.Length, new VoiceEffectSettings(VoiceEffect.Radio, VoiceMuffle.None));

            var output = new byte[VoiceCodec.FrameBytes];
            VoiceCodec.Encode(_radioPcm, ref _radioPredictor, ref _radioIndex, output);
            return output;
        }
    }
}
