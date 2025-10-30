using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Goobstation.Client.Polls.UI;
using Robust.Client.UserInterface.Controllers;

namespace Content.Goobstation.Client.Polls;

public sealed class PollUIController : UIController, IOnStateExited<GameplayState>, IOnStateExited<LobbyState>
{
    private PollVotingWindow? _window;


    public void OnStateExited(GameplayState state)
    {
        CloseWindow();
    }

    public void OnStateExited(LobbyState state)
    {
        CloseWindow();
    }

    public void TogglePollWindow()
    {
        if (_window?.Disposed != false)
            OpenWindow();
        else
            CloseWindow();
    }

    public void OpenWindow()
    {
        if (_window?.Disposed != false)
        {
            _window = UIManager.CreateWindow<PollVotingWindow>();
            _window.OnClose += () => _window = null;
            _window.OpenCentered();
        }
    }

    public void CloseWindow()
    {
        if (_window?.Disposed == false)
        {
            _window.Close();
        }
        _window = null;
    }
}
