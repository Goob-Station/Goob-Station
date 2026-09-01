// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared.Chat;
using Content.Shared.Mobs.Components;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Goobstation.Shared.Wizard.Rules;
using Content.Goobstation.Common.Wizard.Events;
using Content.Goobstation.Server.Wizard.Systems;
using Content.Shared._vg.TileMovement;
using Content.Shared.Movement.Components;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed class GlobalTileMovementSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IAdminLogManager _log = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly WizardRuleSystem _wizardRule = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    private static readonly EntProtoId GameRule = "GlobalTileMovement";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlobalTileToggleEvent>(OnGlobalTileToggle);
        SubscribeLocalEvent<GlobalTileMovementRuleComponent, GameRuleStartedEvent>(OnRuleStarted);
        SubscribeLocalEvent<InputMoverComponent, MapInitEvent>(OnInputMoverMapInit);
    }

    private void OnInputMoverMapInit(Entity<InputMoverComponent> ent, ref MapInitEvent args)
    {
        if (IsRuleActive())
            EnsureComp<TileMovementComponent>(ent);
    }

    public bool IsRuleActive()
    {
        var query = EntityQueryEnumerator<GlobalTileMovementRuleComponent>();
        while (query.MoveNext(out _))
            return true;

        return false;
    }

    private void OnGlobalTileToggle(GlobalTileToggleEvent ev)
    {
        if (IsRuleActive())
            return;

        _gameTicker.StartGameRule(GameRule);

        _configurationManager.SetCVar(CCVars.MovementMobPushing, true); // lmao

        var message = Loc.GetString("global-tile-movement-message");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToAll(ChatChannel.Radio, message, wrappedMessage, default, false, true, Color.Red);
        _audio.PlayGlobal(ev.Sound, Filter.Broadcast(), true);
        _log.Add(LogType.EventRan, LogImpact.Extreme, $"Tile movement has been globally toggled via wizard spellbook.");
    }

    private void OnRuleStarted(Entity<GlobalTileMovementRuleComponent> ent, ref GameRuleStartedEvent args)
    {
        var map = _wizardRule.GetTargetMap();

        if (map == null)
            return;

        var entities = new HashSet<Entity<InputMoverComponent>>();
        _lookup.GetEntitiesOnMap(Transform(map.Value).MapID, entities);
        foreach (var (uid, _) in entities)
        {
            if (TerminatingOrDeleted(uid))
                continue;

            EnsureComp<TileMovementComponent>(uid);
        }
    }
}
