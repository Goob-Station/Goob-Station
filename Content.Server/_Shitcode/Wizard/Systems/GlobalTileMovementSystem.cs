using Content.Server._Goobstation.Wizard.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.EventSpells;
using Content.Shared._Lavaland.Mobs.Components;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Ghost.Roles.Events;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class GlobalTileMovementSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IAdminLogManager _log = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly WizardRuleSystem _wizardRuleSystem = default!;
    private static readonly EntProtoId GameRule = "GlobalTileMovement";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlobalTileToggleEvent>(OnGlobalTileToggle);
        SubscribeLocalEvent<GlobalTileMovementRuleComponent, GameRuleStartedEvent>(OnRuleStarted);
        SubscribeLocalEvent<GhostRoleSpawnerUsedEvent>(OnGhostRoleSpawnerUsed);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }
    public bool GlobalTileMovementIsActive()
    {
        var query = EntityQueryEnumerator<GlobalTileMovementRuleComponent, ActiveGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out _, out _, out _, out _))
        {
            return true;
        }

        return false;
    }

    private void OnGlobalTileToggle(GlobalTileToggleEvent ev)
    {
        if (GlobalTileMovementIsActive())
            return;

        _gameTicker.StartGameRule(GameRule);

        var message = Loc.GetString("global-tile-movement-message");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToAll(ChatChannel.Radio, message, wrappedMessage, default, false, true, Color.Red);
        _audio.PlayGlobal(ev.Sound, Filter.Broadcast(), true);
        _log.Add(LogType.EventRan, LogImpact.Extreme, $"Tile movement has been globally toggled via wizard spellbook.");
    }

    private void OnRuleStarted(Entity<GlobalTileMovementRuleComponent> ent, ref GameRuleStartedEvent args)
    {
        var map = _wizardRuleSystem.GetTargetMap();

        if (map == null)
            return;

        var entities = new HashSet<Entity<MobStateComponent, MindContainerComponent>>();
        _lookup.GetEntitiesOnMap<MobStateComponent, MindContainerComponent>(Transform(map.Value).MapID, entities);
        foreach (var (uid, _, _) in entities)
        {
            if (TerminatingOrDeleted(uid))
                continue;

            EnsureComp<HierophantBeatComponent>(uid);
        }
    }

    private void OnGhostRoleSpawnerUsed(GhostRoleSpawnerUsedEvent args)
    {
        if (!GlobalTileMovementIsActive())
            return;

        EnsureComp<HierophantBeatComponent>(args.Spawned);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent ev)
    {
        if (!GlobalTileMovementIsActive()
            || !ev.LateJoin
            || TerminatingOrDeleted(ev.Mob))
            return;

        EnsureComp<HierophantBeatComponent>(ev.Mob);
    }
}
