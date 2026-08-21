using System.Numerics;
using System.Linq;
using System.Text.Json;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Twitch;
using Content.Server.Ghost.Roles.Components;
using Content.Server.NPC.HTN;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsChatCritterSystem : EntitySystem, ITwitchBitsAction
{
    private static readonly ResPath OAuthPath = new("/twitch_chat_oauth.json");

    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly SharedGodmodeSystem _godmode = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly TwitchPairingSystem _pairings = default!;
    [Dependency] private readonly IResourceManager _resources = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;
    [Dependency] private readonly ViewSubscriberSystem _views = default!;

    private readonly Dictionary<string, ActiveCritter> _active = new(StringComparer.Ordinal);
    private TwitchChatIrcClient? _chat;
    private TwitchChatOAuthManager? _oauth;
    private string _botLogin = string.Empty;
    private string _clientId = string.Empty;
    private string _configuredRefreshToken = string.Empty;

    public string Id => "chat-critter";
    public string DisplayName => "Start Chat Critter";
    public string DisplayDescription => "Let Twitch chat control an immortal mouse with up, down, left, right, and bite.";
    public string Category => "Special";
    public string Sku => "ss14-chat-critter";

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
        SubscribeNetworkEvent<TwitchChatCritterCloseEvent>(OnClose);
        SubscribeLocalEvent<TwitchPairingChangedEvent>(OnPairingChanged);

        _botLogin = _configuration.GetCVar(GoobCVars.TwitchChatBotLogin).Trim();
        _clientId = _configuration.GetCVar(GoobCVars.TwitchChatClientId).Trim();
        _configuredRefreshToken = _configuration.GetCVar(GoobCVars.TwitchChatRefreshToken).Trim();
        var accessToken = _configuration.GetCVar(GoobCVars.TwitchChatOauthToken).Trim();
        var refreshToken = _configuredRefreshToken;
        LoadOAuthState(ref accessToken, ref refreshToken);
        if (string.IsNullOrEmpty(_botLogin) || string.IsNullOrEmpty(accessToken))
            return;

        ResetChat(accessToken);
        _oauth = new TwitchChatOAuthManager(
            accessToken,
            refreshToken,
            _clientId,
            _configuration.GetCVar(GoobCVars.TwitchChatClientSecret),
            (newAccessToken, newRefreshToken) => _taskManager.RunOnMainThread(() =>
            {
                SaveOAuthState(newAccessToken, newRefreshToken);
                ResetChat(newAccessToken);
            }),
            warning => _taskManager.RunOnMainThread(() => Log.Warning(warning)));
        _oauth.Start();
    }

    public override void Shutdown()
    {
        foreach (var channelId in _active.Keys.ToArray())
            Cleanup(channelId);
        if (_chat != null)
            _ = _chat.DisposeAsync();
        if (_oauth != null)
            _ = _oauth.DisposeAsync();
        base.Shutdown();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (channelId, state) in _active.ToArray())
        {
            if (!Exists(state.Critter) || _timing.CurTime >= state.ExpiresAt)
            {
                Cleanup(channelId);
                continue;
            }

            if (state.StopAt == TimeSpan.Zero || _timing.CurTime < state.StopAt)
                continue;

            if (TryComp<PhysicsComponent>(state.Critter, out var physics))
                _physics.SetLinearVelocity(state.Critter, Vector2.Zero, body: physics);
            state.StopAt = TimeSpan.Zero;
        }
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        if (_active.TryGetValue(context.ChannelId, out var active) && Exists(active.Critter))
            return TwitchBitsActionValidity.Invalid("Chat Critter is already active.");

        if (!_pairings.TryGetPairing(context.ChannelId, out var pairing))
            return TwitchBitsActionValidity.Invalid("This Twitch channel is not linked to an SS14 player.");

        if (_chat == null)
            return TwitchBitsActionValidity.Invalid("Configure the Twitch bot login and OAuth token before starting Chat Critter.");

        if (!_chat.IsConnected(pairing.ChannelLogin))
            return TwitchBitsActionValidity.Invalid("The SS14 server is still connecting to this Twitch channel's chat.");

        if (!_players.TryGetSessionByEntity(target, out _))
            return TwitchBitsActionValidity.Invalid("The streamer session could not be found.");

        return TwitchBitsActionValidity.Valid;
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        if (!IsCurrentlyValid(target, context).IsValid ||
            !_players.TryGetSessionByEntity(target, out var session) ||
            !_pairings.TryGetPairing(context.ChannelId, out var pairing))
        {
            return false;
        }

        var critter = Spawn("MobMouse", Transform(target).Coordinates.Offset(Vector2.UnitX));
        RemComp<HTNComponent>(critter);
        RemComp<GhostRoleComponent>(critter);
        RemComp<GhostTakeoverAvailableComponent>(critter);
        _godmode.EnableGodmode(critter);
        _meta.SetEntityName(critter, "Chat Critter");
        _views.AddViewSubscriber(critter, session);

        _active[context.ChannelId] = new ActiveCritter(
            pairing.ChannelLogin,
            critter,
            session,
            _timing.CurTime + TimeSpan.FromSeconds(Math.Clamp(
                _configuration.GetCVar(GoobCVars.TwitchChatCritterDuration),
                30,
                1800)));
        RaiseNetworkEvent(new TwitchChatCritterOpenEvent(GetNetEntity(critter)), session);
        return true;
    }

    private void HandleCommand(string channelLogin, string viewer, string command)
    {
        var state = _active.Values.FirstOrDefault(active =>
            string.Equals(active.ChannelLogin, channelLogin, StringComparison.OrdinalIgnoreCase));
        if (state == null || !Exists(state.Critter) || _timing.CurTime >= state.ExpiresAt)
            return;

        if (command == "bite")
        {
            Bite(state.Critter);
        }
        else if (TryComp<PhysicsComponent>(state.Critter, out var physics))
        {
            var direction = command switch
            {
                "up" => Vector2.UnitY,
                "down" => -Vector2.UnitY,
                "left" => -Vector2.UnitX,
                "right" => Vector2.UnitX,
                _ => Vector2.Zero,
            };
            if (direction != Vector2.Zero)
            {
                _physics.SetLinearVelocity(state.Critter, direction * 4f, body: physics);
                state.StopAt = _timing.CurTime + TimeSpan.FromSeconds(0.45);
            }
        }

        RaiseNetworkEvent(new TwitchChatCritterCommandEvent(viewer, command), state.Streamer);
    }

    private void Bite(EntityUid critter)
    {
        if (!TryComp<MeleeWeaponComponent>(critter, out var weapon))
            return;

        var target = _lookup.GetEntitiesInRange<MobStateComponent>(Transform(critter).Coordinates, 1.25f)
            .Where(entity => entity.Owner != critter)
            .OrderBy(entity => (Transform(entity).Coordinates.Position - Transform(critter).Coordinates.Position).LengthSquared())
            .FirstOrDefault();
        if (target.Owner.Valid)
            _melee.AttemptLightAttack(critter, critter, weapon, target.Owner);
    }

    private void OnClose(TwitchChatCritterCloseEvent message, EntitySessionEventArgs args)
    {
        var critter = GetEntity(message.Critter);
        var state = _active.Values.FirstOrDefault(active =>
            active.Critter == critter && args.SenderSession == active.Streamer);
        if (state != null && Exists(critter))
            _views.RemoveViewSubscriber(critter, args.SenderSession);
    }

    private void OnPairingChanged(TwitchPairingChangedEvent args)
    {
        if (args.Paired)
        {
            _chat?.JoinChannel(args.ChannelLogin);
            return;
        }

        _chat?.LeaveChannel(args.ChannelLogin);
        Cleanup(args.ChannelId);
    }

    private void ResetChat(string accessToken)
    {
        var previous = _chat;
        _chat = new TwitchChatIrcClient(_botLogin, accessToken, (channel, viewer, command) =>
            _taskManager.RunOnMainThread(() => HandleCommand(channel, viewer, command)));
        foreach (var pairing in _pairings.Pairings)
            _chat.JoinChannel(pairing.ChannelLogin);
        _chat.Start();
        if (previous != null)
            _ = previous.DisposeAsync();
    }

    private void LoadOAuthState(ref string accessToken, ref string refreshToken)
    {
        if (string.IsNullOrEmpty(_configuredRefreshToken) ||
            !_resources.UserData.TryReadAllText(OAuthPath, out var json))
        {
            return;
        }

        try
        {
            var state = JsonSerializer.Deserialize<StoredOAuthState>(json);
            if (state == null ||
                state.ClientId != _clientId ||
                state.SeedRefreshToken != _configuredRefreshToken ||
                string.IsNullOrWhiteSpace(state.AccessToken) ||
                string.IsNullOrWhiteSpace(state.RefreshToken))
            {
                return;
            }

            accessToken = state.AccessToken;
            refreshToken = state.RefreshToken;
        }
        catch (JsonException exception)
        {
            Log.Warning($"Could not load Twitch chat OAuth state: {exception.Message}");
        }
    }

    private void SaveOAuthState(string accessToken, string refreshToken)
    {
        var state = new StoredOAuthState(_clientId, _configuredRefreshToken, accessToken, refreshToken);
        _resources.UserData.WriteAllText(OAuthPath, JsonSerializer.Serialize(state));
    }

    private void Cleanup(string channelId)
    {
        if (!_active.Remove(channelId, out var state))
            return;

        RaiseNetworkEvent(new TwitchChatCritterClosedEvent(), state.Streamer);
        if (Exists(state.Critter))
        {
            _views.RemoveViewSubscriber(state.Critter, state.Streamer);
            QueueDel(state.Critter);
        }
    }

    private sealed class ActiveCritter(
        string channelLogin,
        EntityUid critter,
        ICommonSession streamer,
        TimeSpan expiresAt)
    {
        public string ChannelLogin { get; } = channelLogin;
        public EntityUid Critter { get; } = critter;
        public ICommonSession Streamer { get; } = streamer;
        public TimeSpan ExpiresAt { get; } = expiresAt;
        public TimeSpan StopAt { get; set; }
    }

    private sealed record StoredOAuthState(
        string ClientId,
        string SeedRefreshToken,
        string AccessToken,
        string RefreshToken);
}
