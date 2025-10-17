using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared._Goobstation.Wizard;
using Robust.Server.Audio;
using Content.Shared._Goobstation.Wizard.EventSpells;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class MobCollisionSpellSystem : SharedMobCollisionSpellSystem
{
    /// <inheritdoc/>

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IAdminLogManager _log = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager CfgManager = default!;

    private static readonly EntProtoId GameRule = "MobCollisionSpell";

    public override void Initialize()
    {
        SubscribeLocalEvent<MobColissionSpellEvent>(OnCastSpell);
    }
    private void OnCastSpell(MobColissionSpellEvent ev)
    {
        if (MobCollisionEnabled() || CfgManager.GetCVar(CCVars.MovementMobPushing))
            return;

        _gameTicker.StartGameRule(GameRule);

        var message = "Out of my way fatty";
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToAll(ChatChannel.Radio, message, wrappedMessage, default, false, true, Color.Red);
        _audio.PlayGlobal(ev.Sound, Filter.Broadcast(), true);

        _log.Add(LogType.EventRan, LogImpact.Extreme, $"Mob collision have been enabled via wizard spellbook.");
    }


}
