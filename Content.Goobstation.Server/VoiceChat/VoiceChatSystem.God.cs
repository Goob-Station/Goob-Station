using Content.Goobstation.Shared.VoiceChat;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Verbs;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.VoiceChat;

public enum VoiceGodMode : byte
{
    Player,
    Radius,
    Department,
    Station,
}

public readonly record struct VoiceGodTarget(VoiceGodMode Mode, string Description, NetUserId Player = default, float Radius = 0f, string Department = "");

public sealed partial class VoiceChatSystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedMindSystem _minds = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;

    private static readonly TimeSpan GodTailDelay = TimeSpan.FromMilliseconds(60);
    private static readonly TimeSpan GodFrameDuration = TimeSpan.FromMilliseconds(20);
    private static readonly TimeSpan GodTransmissionGap = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan GodCueGap = TimeSpan.FromSeconds(3);
    private static readonly SpriteSpecifier GodVerbIcon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/sentient.svg.192dpi.png"));
    private static readonly Color GodCueColor = Color.FromHex("#FFE9A8");
    private const int GodTailFrames = 200;

    private readonly Dictionary<NetUserId, GodVoice> _godVoices = new();

    public VoiceGodTarget GodTargetPlayer(ICommonSession target)
    {
        return new VoiceGodTarget(VoiceGodMode.Player, target.Name, target.UserId);
    }

    public VoiceGodTarget GodTargetRadius(float radius)
    {
        return new VoiceGodTarget(VoiceGodMode.Radius, Loc.GetString("voice-god-target-radius", ("radius", radius)), Radius: radius);
    }

    public VoiceGodTarget GodTargetDepartment(DepartmentPrototype department)
    {
        return new VoiceGodTarget(VoiceGodMode.Department, Loc.GetString("voice-god-target-department", ("department", Loc.GetString(department.Name))), Department: department.ID);
    }

    public VoiceGodTarget GodTargetStation()
    {
        return new VoiceGodTarget(VoiceGodMode.Station, Loc.GetString("voice-god-target-station"));
    }

    public bool IsGodVoiceTarget(NetUserId admin, NetUserId target)
    {
        return _godVoices.TryGetValue(admin, out var god) &&
               god.Target.Mode == VoiceGodMode.Player &&
               god.Target.Player == target;
    }

    public void StartGodVoice(ICommonSession admin, VoiceGodTarget target, bool hearSelf = false)
    {
        _godVoices.Remove(admin.UserId);
        _godVoices[admin.UserId] = new GodVoice(admin.UserId, target, AllocateStreamId(), hearSelf);

        _adminLog.Add(LogType.AdminMessage, LogImpact.Low, $"{admin.Name} started speaking to {target.Description} as the voice of god");

        var message = _voice.IsWebConnected(admin.UserId) ? "voice-god-started" : "voice-god-started-offline";
        _chatManager.DispatchServerMessage(admin, Loc.GetString(message, ("target", target.Description)));

        if (hearSelf)
            _chatManager.DispatchServerMessage(admin, Loc.GetString("voice-god-hear-self"));
    }

    public void StopGodVoice(ICommonSession admin)
    {
        if (!_godVoices.Remove(admin.UserId, out var god))
            return;

        _adminLog.Add(LogType.AdminMessage, LogImpact.Low, $"{admin.Name} stopped speaking to {god.Target.Description} as the voice of god");
        _chatManager.DispatchServerMessage(admin, Loc.GetString("voice-god-stopped"));
    }

    private void InitializeGod()
    {
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGodVerbs);
        _player.PlayerStatusChanged += OnGodPlayerStatusChanged;
    }

    private void ShutdownGod()
    {
        _player.PlayerStatusChanged -= OnGodPlayerStatusChanged;
    }

    private bool TryHandleGodFrame(ICommonSession session, VoiceWebFrame frame, TimeSpan now)
    {
        if (!_godVoices.TryGetValue(frame.User, out var god))
            return false;

        if (!_adminManager.HasAdminFlag(session, AdminFlags.Admin) || !CollectGodRecipients(session, god))
        {
            StopGodVoice(session);
            return false;
        }

        var speaker = GetSpeaker(frame.User);
        var levels = speaker.Measure(frame.Payload, out _);
        speaker.LastAttempt = now;
        SendSelf(session, speaker, levels, VoiceSelfFlags.God);
        _voiceLog.Record(session, frame, VoiceLogFlags.God, Loc.GetString("voice-god-log-name", ("target", god.Target.Description)), string.Empty);

        if (!speaker.Decoded)
            return true;

        SendGodFrame(god, speaker.CopyPcm(), now, true);
        god.NextTail = now + GodTailDelay;
        god.TailFrames = GodTailFrames;
        return true;
    }

    private bool CollectGodRecipients(ICommonSession admin, GodVoice god)
    {
        god.Recipients.Clear();

        if (god.HearSelf)
            god.Recipients.Add(admin);

        if (god.Target.Mode == VoiceGodMode.Player)
        {
            if (!_player.TryGetSessionById(god.Target.Player, out var target) ||
                target.Status is SessionStatus.Disconnected or SessionStatus.Zombie)
            {
                return false;
            }

            if (!god.Recipients.Contains(target))
                god.Recipients.Add(target);

            return true;
        }

        var origin = MapCoordinates.Nullspace;
        if (god.Target.Mode == VoiceGodMode.Radius)
        {
            if (admin.AttachedEntity is not { } adminEntity)
                return true;

            origin = _transform.GetMapCoordinates(adminEntity);
        }

        foreach (var session in _player.Sessions)
        {
            if (session == admin ||
                session.Status != SessionStatus.InGame ||
                session.AttachedEntity is not { } listener)
            {
                continue;
            }

            var include = god.Target.Mode switch
            {
                VoiceGodMode.Radius => IsWithin(listener, origin, god.Target.Radius),
                VoiceGodMode.Department => IsInDepartment(session, god.Target.Department),
                _ => true,
            };

            if (include)
                god.Recipients.Add(session);
        }

        return true;
    }

    private bool IsWithin(EntityUid listener, MapCoordinates origin, float radius)
    {
        var position = _transform.GetMapCoordinates(listener);
        return position.MapId == origin.MapId &&
               (position.Position - origin.Position).LengthSquared() <= radius * radius;
    }

    private bool IsInDepartment(ICommonSession session, string department)
    {
        return _minds.TryGetMind(session.UserId, out var mind) &&
               _jobs.MindTryGetJobId(mind.Value.Owner, out var job) &&
               job is { } jobId &&
               _prototypes.TryIndex(new ProtoId<DepartmentPrototype>(department), out var prototype) &&
               prototype.Roles.Contains(jobId);
    }

    private void UpdateGod(TimeSpan now)
    {
        foreach (var god in _godVoices.Values)
        {
            if (god.TailFrames <= 0 || now < god.NextTail)
                continue;

            while (god.TailFrames > 0 && god.NextTail <= now)
            {
                god.TailFrames--;
                god.NextTail += GodFrameDuration;
                if (!SendGodFrame(god, new short[VoiceCodec.FrameSamples], now, false))
                    god.TailFrames = 0;
            }
        }
    }

    private bool SendGodFrame(GodVoice god, short[] pcm, TimeSpan now, bool speech)
    {
        god.Effect.Process(pcm);
        if (!speech && VoiceGodEffect.IsSilent(pcm))
            return false;

        var payload = god.Encoder.Encode(pcm, _format);

        var flags = now - god.LastSent > GodTransmissionGap ? VoiceCodec.FlagTransmissionStart : (byte) 0;
        god.LastSent = now;

        god.Channels.Clear();
        foreach (var recipient in god.Recipients)
        {
            if (recipient.Status == SessionStatus.Disconnected)
                continue;

            god.Channels.Add(recipient.Channel);

            if (god.InfoSent.Add(recipient.UserId))
            {
                _net.ServerSendMessage(new MsgVoiceSpeakerInfo
                {
                    Speaker = god.StreamId,
                    Name = Loc.GetString("voice-god-name"),
                    Channel = string.Empty,
                }, recipient.Channel);
            }

            if (speech)
                SendGodCue(god, recipient, now);
        }

        if (god.Channels.Count == 0)
            return true;

        _net.ServerSendToMany(new MsgVoiceFrame
        {
            Source = NetEntity.Invalid,
            Speaker = god.StreamId,
            Sequence = god.Sequence++,
            Flags = flags,
            Route = VoiceRoute.God,
            Format = _format,
            Global = true,
            Range = 0f,
            Payload = payload,
        }, god.Channels);
        return true;
    }

    private void SendGodCue(GodVoice god, ICommonSession recipient, TimeSpan now)
    {
        var cue = !god.LastHeard.TryGetValue(recipient.UserId, out var last) || now - last > GodCueGap;
        god.LastHeard[recipient.UserId] = now;
        if (!cue)
            return;

        RaiseNetworkEvent(new VoiceGodCueEvent(), recipient);
        if (recipient.UserId == god.Admin)
            return;

        var message = Loc.GetString("voice-god-cue");
        var wrapped = $"[italic]{FormattedMessage.EscapeText(message)}[/italic]";
        _chatManager.ChatMessageToOne(ChatChannel.Local, message, wrapped, EntityUid.Invalid, false, recipient.Channel, GodCueColor);
    }

    private void OnGodVerbs(GetVerbsEvent<Verb> args)
    {
        if (args.User == args.Target ||
            !TryComp<ActorComponent>(args.User, out var adminActor) ||
            !TryComp<ActorComponent>(args.Target, out var targetActor) ||
            !_adminManager.HasAdminFlag(adminActor.PlayerSession, AdminFlags.Admin))
        {
            return;
        }

        var admin = adminActor.PlayerSession;
        var target = targetActor.PlayerSession;
        var active = IsGodVoiceTarget(admin.UserId, target.UserId);

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString(active ? "voice-god-verb-stop" : "voice-god-verb"),
            Category = VerbCategory.Admin,
            Icon = GodVerbIcon,
            Act = () =>
            {
                if (active)
                    StopGodVoice(admin);
                else
                    StartGodVoice(admin, GodTargetPlayer(target));
            },
        });
    }

    private void OnGodPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Disconnected)
            return;

        _godVoices.Remove(args.Session.UserId);
        foreach (var god in _godVoices.Values)
        {
            god.InfoSent.Remove(args.Session.UserId);
            god.LastHeard.Remove(args.Session.UserId);
        }
    }

    private sealed class GodVoice(NetUserId admin, VoiceGodTarget target, ushort streamId, bool hearSelf)
    {
        public readonly NetUserId Admin = admin;
        public readonly VoiceGodTarget Target = target;
        public readonly ushort StreamId = streamId;
        public readonly bool HearSelf = hearSelf;
        public readonly VoiceGodEffect Effect = new();
        public readonly VoiceEncoder Encoder = new();
        public readonly List<ICommonSession> Recipients = new();
        public readonly List<INetChannel> Channels = new();
        public readonly HashSet<NetUserId> InfoSent = new();
        public readonly Dictionary<NetUserId, TimeSpan> LastHeard = new();
        public ushort Sequence;
        public TimeSpan LastSent;
        public TimeSpan NextTail;
        public int TailFrames;
    }
}
