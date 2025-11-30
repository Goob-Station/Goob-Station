using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Chat;
using Content.Shared.Database;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class SpaceWindSystem : EntitySystem
{

    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IAdminLogManager _log = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpaceWindSpellEvent>(OnCastSpell);
    }

    private void OnCastSpell(SpaceWindSpellEvent ev)
    {
        if (_atmosphereSystem.GetSpaceWind())
            return;

        _atmosphereSystem.SetSpaceWind(true);

        var message = "Its just a little breeze";
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToAll(ChatChannel.Radio, message, wrappedMessage, default, false, true, Color.Red);
        _audio.PlayGlobal(ev.Sound, Filter.Broadcast(), true);
        _log.Add(LogType.EventRan, LogImpact.Extreme, $"Space Wind have been enabled via wizard spellbook.");
    }
}
