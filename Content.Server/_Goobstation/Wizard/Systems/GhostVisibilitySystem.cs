using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Eye;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class GhostVisibilitySystem : EntitySystem
{
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IAdminLogManager _log = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    [ViewVariables]
    public bool GhostsVisible { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonGhostsEvent>(OnSummonGhosts);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        GhostsVisible = false;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        GhostsVisible = false;
    }

    private void OnSummonGhosts(SummonGhostsEvent ev)
    {
        if (GhostsVisible)
            return;

        GhostsVisible = true;

        var entityQuery = EntityQueryEnumerator<GhostComponent, VisibilityComponent>();
        while (entityQuery.MoveNext(out var uid, out var ghost, out var vis))
        {
            if (ghost.CanGhostInteract)
                continue;

            _visibilitySystem.AddLayer((uid, vis), (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RemoveLayer((uid, vis), (int) VisibilityFlags.Ghost, false);

            _visibilitySystem.RefreshVisibility(uid, visibilityComponent: vis);
        }

        var message = Loc.GetString("ghosts-summoned-message");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToAll(ChatChannel.Radio, message, wrappedMessage, default, false, true, Color.Red);
        _audio.PlayGlobal(ev.Sound, Filter.Broadcast(), true);

        _log.Add(LogType.EventRan, LogImpact.Extreme, $"Ghosts have been summoned via wizard spellbook.");
    }

    public bool IsVisible(GhostComponent component)
    {
        if (!GhostsVisible)
            return false;

        return !component.CanGhostInteract;
    }
}
