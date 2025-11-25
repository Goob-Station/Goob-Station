using Content.Client.Lobby;
using Content.Goobstation.Common.CCVar;
using Content.Shared.CCVar;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Client.Patron;

public sealed class PatronSupportUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IUriOpener _uriOpener = default!;

    private PatronSupportWindow? _supportWindow;
    private bool _hasShownThisSession;

    public void OnStateEntered(LobbyState state)
    {
        if (_hasShownThisSession)
            return;

        var lastShown = _cfg.GetCVar(GoobCVars.PatronSupportLastShown);
        var now = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(lastShown))
        {
            if (DateTime.TryParse(lastShown, out var lastShownDate))
            {
                var daysSinceLastShown = (now - lastShownDate).TotalDays;
                if (daysSinceLastShown < _cfg.GetCVar(GoobCVars.PatronAskSupport))
                    return;
            }
        }

        _hasShownThisSession = true;
        ShowSupportWindow();
    }

    public void OnStateExited(LobbyState state)
    {
        if (_supportWindow == null)
            return;

        _supportWindow.OnClose -= OnWindowClosed;
        _supportWindow.Dispose();
        _supportWindow = null;
    }

    private void ShowSupportWindow()
    {
        if (_supportWindow != null)
            return;

        _supportWindow = UIManager.CreateWindow<PatronSupportWindow>();
        _supportWindow.OnClose += OnWindowClosed;

        _supportWindow.PatreonButton.OnPressed += _ =>
        {
            var patreonLink = _cfg.GetCVar(CCVars.InfoLinksPatreon);
            if (!string.IsNullOrEmpty(patreonLink))
                _uriOpener.OpenUri(new Uri(patreonLink));
            _supportWindow?.Close();
        };

        _supportWindow.OpenCenteredLeft();
    }

    private void OnWindowClosed()
    {
        _cfg.SetCVar(GoobCVars.PatronSupportLastShown, DateTime.UtcNow.ToString("O"));
        _cfg.SaveToFile();
    }
}
