using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Shared.ActionBlocker;
using Content.Shared.Ghost;
using Content.Shared.Input;
using Content.Shared.Mobs.Systems;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
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

    private static readonly TimeSpan SpeakingTimeout = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan TransmissionGap = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan PermissionRecheck = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan RefusalDisplay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan StateRefreshInterval = TimeSpan.FromMilliseconds(500);
    private const float ListenerMargin = 2f;

    private readonly Dictionary<NetUserId, Speaker> _speakers = new();
    private readonly HashSet<NetUserId> _adminMuted = new();
    private readonly Dictionary<NetUserId, List<INetChannel>> _listenerCache = new();
    private readonly Stack<List<INetChannel>> _listPool = new();

    private EntityQuery<GhostComponent> _ghostQuery;
    private EntityQuery<EyeComponent> _eyeQuery;
    private EntityQuery<SpeechComponent> _speechQuery;
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

        foreach (var list in _listenerCache.Values)
        {
            list.Clear();
            _listPool.Push(list);
        }

        _listenerCache.Clear();
        _sessions = null;

        while (_voice.TryDequeueEvent(out var ev))
        {
            switch (ev)
            {
                case VoiceWebFrame frame:
                    HandleFrame(frame, now);
                    break;
                case VoiceWebConnectionChanged changed:
                    SendState(changed.User, true);
                    break;
            }
        }

        UpdateSpeaking(now);

        if (now < _nextStateRefresh)
            return;

        _nextStateRefresh = now + StateRefreshInterval;
        foreach (var user in _speakers.Keys)
        {
            SendState(user, false);
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
            session.AttachedEntity is not { } uid ||
            _adminMuted.Contains(frame.User))
        {
            return;
        }

        var speaker = GetSpeaker(frame.User);
        var newTransmission = now - speaker.LastAttempt > TransmissionGap;
        speaker.LastAttempt = now;

        if (!CanTransmit(speaker, uid, now, newTransmission))
            return;

        MarkSpeaking(speaker, uid, now);

        var channels = GetListeners(session, uid);
        if (channels.Count == 0)
            return;

        _net.ServerSendToMany(new MsgVoiceFrame
        {
            Source = GetNetEntity(uid),
            Speaker = speaker.Id,
            Sequence = frame.Sequence,
            Flags = frame.Flags,
            Payload = frame.Payload,
        }, channels);
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

        if (_mobState.IsIncapacitated(uid) || HasComp<MutedComponent>(uid))
            return false;

        return _speechQuery.TryComp(uid, out var speech) && speech.Enabled;
    }

    private void MarkSpeaking(Speaker speaker, EntityUid uid, TimeSpan now)
    {
        speaker.LastFrame = now;
        if (speaker.SpeakingEntity == uid)
            return;

        if (speaker.SpeakingEntity is { } previous && !TerminatingOrDeleted(previous))
            RemComp<VoiceChatSpeakingComponent>(previous);

        speaker.SpeakingEntity = uid;
        EnsureComp<VoiceChatSpeakingComponent>(uid);
    }

    private void UpdateSpeaking(TimeSpan now)
    {
        foreach (var speaker in _speakers.Values)
        {
            if (speaker.SpeakingEntity is not { } uid || now - speaker.LastFrame < SpeakingTimeout)
                continue;

            speaker.SpeakingEntity = null;
            if (!TerminatingOrDeleted(uid))
                RemComp<VoiceChatSpeakingComponent>(uid);
        }
    }

    private List<INetChannel> GetListeners(ICommonSession speakerSession, EntityUid speakerUid)
    {
        if (_listenerCache.TryGetValue(speakerSession.UserId, out var cached))
            return cached;

        var channels = _listPool.Count > 0 ? _listPool.Pop() : new List<INetChannel>();
        _listenerCache[speakerSession.UserId] = channels;

        var origin = _transform.GetMapCoordinates(speakerUid);
        if (origin.MapId == MapId.Nullspace)
            return channels;

        var ghostSpeaker = _ghostQuery.HasComp(speakerUid);
        var maxDistance = _range + ListenerMargin;
        var maxDistanceSquared = maxDistance * maxDistance;

        _sessions ??= _player.Sessions;
        foreach (var session in _sessions)
        {
            if (session.Status != SessionStatus.InGame ||
                session.AttachedEntity is not { } listener ||
                !_voice.Receives(session.UserId))
            {
                continue;
            }

            if (session == speakerSession)
            {
                if (_voice.HearsSelf(session.UserId))
                    channels.Add(session.Channel);
                continue;
            }

            if (ghostSpeaker && !_ghostQuery.HasComp(listener))
                continue;

            var position = GetListenerCoordinates(listener);
            if (position.MapId != origin.MapId ||
                Vector2.DistanceSquared(position.Position, origin.Position) > maxDistanceSquared)
            {
                continue;
            }

            channels.Add(session.Channel);
        }

        return channels;
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
            return new VoiceWebState(string.Empty, false, false, _adminMuted.Contains(user), false);

        if (session.Status != SessionStatus.InGame || session.AttachedEntity is not { } uid)
            return new VoiceWebState(session.Name, false, false, _adminMuted.Contains(user), false);

        var refused = speaker.CheckedEntity == uid &&
                      !speaker.Allowed &&
                      _timing.RealTime - speaker.LastAttempt < RefusalDisplay;

        return new VoiceWebState(
            session.Name,
            true,
            !refused && CanSpeakPassive(uid),
            _adminMuted.Contains(user),
            speaker.PushToTalk);
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

    private sealed class Speaker(NetUserId user, ushort id)
    {
        public readonly NetUserId User = user;
        public readonly ushort Id = id;
        public bool PushToTalk;
        public bool Allowed;
        public EntityUid? CheckedEntity;
        public TimeSpan CheckedAt;
        public TimeSpan LastAttempt;
        public TimeSpan LastFrame;
        public EntityUid? SpeakingEntity;
        public VoiceWebState? LastState;
    }
}
