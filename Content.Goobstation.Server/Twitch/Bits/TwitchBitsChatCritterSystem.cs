using System.Numerics;
using System.Linq;
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
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsChatCritterSystem : EntitySystem, ITwitchBitsAction
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly SharedGodmodeSystem _godmode = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;
    [Dependency] private readonly ViewSubscriberSystem _views = default!;

    private TwitchChatIrcClient? _chat;
    private EntityUid? _critter;
    private ICommonSession? _streamer;
    private TimeSpan _expiresAt;
    private TimeSpan _stopAt;

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

        var channel = _configuration.GetCVar(GoobCVars.TwitchChatChannelLogin).Trim();
        var botLogin = _configuration.GetCVar(GoobCVars.TwitchChatBotLogin).Trim();
        var oauthToken = _configuration.GetCVar(GoobCVars.TwitchChatOauthToken).Trim();
        if (!string.IsNullOrEmpty(channel) &&
            !string.IsNullOrEmpty(botLogin) &&
            !string.IsNullOrEmpty(oauthToken))
        {
            _chat = new TwitchChatIrcClient(channel, botLogin, oauthToken, (viewer, command) =>
                _taskManager.RunOnMainThread(() => HandleCommand(viewer, command)));
            _chat.Start();
        }
    }

    public override void Shutdown()
    {
        _chat?.Stop();
        Cleanup();
        base.Shutdown();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_critter is not { } critter)
            return;

        if (!Exists(critter) || _timing.CurTime >= _expiresAt)
        {
            Cleanup();
            return;
        }

        if (_stopAt != TimeSpan.Zero && _timing.CurTime >= _stopAt)
        {
            if (TryComp<PhysicsComponent>(critter, out var physics))
                _physics.SetLinearVelocity(critter, Vector2.Zero, body: physics);
            _stopAt = TimeSpan.Zero;
        }
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        if (_critter is { } critter && Exists(critter))
            return TwitchBitsActionValidity.Invalid("Chat Critter is already active.");

        if (_chat == null)
            return TwitchBitsActionValidity.Invalid("Configure the Twitch chat channel, bot login, and OAuth token before starting Chat Critter.");

        if (!_chat.IsConnected)
            return TwitchBitsActionValidity.Invalid("The SS14 server is still connecting to Twitch chat.");

        if (!_players.TryGetSessionByEntity(target, out _))
            return TwitchBitsActionValidity.Invalid("The streamer session could not be found.");

        return TwitchBitsActionValidity.Valid;
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        if (!IsCurrentlyValid(target, context).IsValid ||
            !_players.TryGetSessionByEntity(target, out var session))
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

        _critter = critter;
        _streamer = session;
        _expiresAt = _timing.CurTime + TimeSpan.FromSeconds(Math.Clamp(
            _configuration.GetCVar(GoobCVars.TwitchChatCritterDuration),
            30,
            1800));
        RaiseNetworkEvent(new TwitchChatCritterOpenEvent(GetNetEntity(critter)), session);
        return true;
    }

    private void HandleCommand(string viewer, string command)
    {
        if (_critter is not { } critter || !Exists(critter) || _timing.CurTime >= _expiresAt)
            return;

        if (command == "bite")
        {
            Bite(critter);
        }
        else if (TryComp<PhysicsComponent>(critter, out var physics))
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
                _physics.SetLinearVelocity(critter, direction * 4f, body: physics);
                _stopAt = _timing.CurTime + TimeSpan.FromSeconds(0.45);
            }
        }

        if (_streamer != null)
            RaiseNetworkEvent(new TwitchChatCritterCommandEvent(viewer, command), _streamer);
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
        if (_critter is not { } critter ||
            GetEntity(message.Critter) != critter ||
            args.SenderSession != _streamer)
        {
            return;
        }

        _views.RemoveViewSubscriber(critter, args.SenderSession);
    }

    private void Cleanup()
    {
        if (_streamer != null)
            RaiseNetworkEvent(new TwitchChatCritterClosedEvent(), _streamer);

        if (_critter is { } critter)
        {
            if (_streamer != null && Exists(critter))
                _views.RemoveViewSubscriber(critter, _streamer);
            if (Exists(critter))
                QueueDel(critter);
        }

        _critter = null;
        _streamer = null;
        _expiresAt = TimeSpan.Zero;
        _stopAt = TimeSpan.Zero;
    }
}
