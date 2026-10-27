using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Content.Goobstation.Client.News;
using Content.Goobstation.Client.News.UI;
using Content.Goobstation.Shared.Lobby;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Goobstation.Client.Lobby;

public sealed partial class HullLobbyUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    [Dependency] private NewsManager _news = default!;

    [UISystemDependency] private readonly LobbyRoundInfoSystem? _roundInfo = default;

    private LobbyGui? _lobby;

    public void OnStateEntered(LobbyState state)
    {
        if (state.Lobby is not { } lobby)
            return;

        _lobby = lobby;
        lobby.NewsContainer.RemoveAllChildren();
        lobby.NewsContainer.AddChild(new NewsFeed());

        _news.Changed += UpdateUnread;
        UpdateUnread();
        lobby.SelectTab(_news.UnreadCount == 0);

        if (_roundInfo != null)
        {
            _roundInfo.Updated += UpdateRoundInfo;
            if (_roundInfo.Info is { } info)
                UpdateRoundInfo(info);
        }
    }

    public void OnStateExited(LobbyState state)
    {
        _news.Changed -= UpdateUnread;

        if (_roundInfo != null)
            _roundInfo.Updated -= UpdateRoundInfo;

        _lobby = null;
    }

    private void UpdateUnread()
    {
        _lobby?.SetNewsUnread(_news.UnreadCount > 0);
    }

    private void UpdateRoundInfo(LobbyRoundInfoEvent info)
    {
        _lobby?.SetRoundInfo(info.MapName, info.ModeTitle, info.RoundId, info.PlayerCount, info.ReadyCount);
    }
}
