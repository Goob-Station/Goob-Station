// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.GameTicking.Managers;
using Content.Shared.GameTicking;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Content.Client._Pirate.RoundEnd.PhotoAlbum; // Pirate: camera
using Robust.Client.UserInterface; // Pirate: camera

namespace Content.Client.RoundEnd;

[UsedImplicitly]
public sealed class RoundEndSummaryUIController : UIController,
    IOnSystemLoaded<ClientGameTicker>,
    IOnSystemChanged<PhotoAlbumSystem>
{
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IFileDialogManager _fileDialogManager = default!; // Pirate: camera
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!; // Pirate: camera

    private RoundEndSummaryWindow? _window;
    private PhotoAlbumSystem? _photoAlbum; // Pirate: camera

    private void ToggleScoreboardWindow(ICommonSession? session = null)
    {
        if (_window == null)
            return;

        if (_window.IsOpen)
        {
            _window.Close();
        }
        else
        {
            _window.OpenCenteredRight();
            _window.MoveToFront();
        }
    }

    public void OpenRoundEndSummaryWindow(RoundEndMessageEvent message)
    {
        // Don't open duplicate windows (mainly for replays).
        if (_window?.RoundId == message.RoundId)
            return;

        _window = new RoundEndSummaryWindow(message.GamemodeTitle, message.RoundEndText,
            message.RoundDuration, message.RoundId, message.AllPlayersEndInfo, EntityManager, _fileDialogManager); // Pirate: camera
    }

    public void OnSystemLoaded(ClientGameTicker system)
    {
        _input.SetInputCommand(ContentKeyFunctions.ToggleRoundEndSummaryWindow,
            InputCmdHandler.FromDelegate(ToggleScoreboardWindow));
    }

    #region Pirate: camera
    public void OnSystemLoaded(PhotoAlbumSystem system)
    {
        _photoAlbum = system;
        _photoAlbum.AlbumsUpdated += OnPhotoAlbumsUpdated;
    }

    public void OnSystemUnloaded(PhotoAlbumSystem system)
    {
        if (_photoAlbum is null)
            return;

        _photoAlbum.AlbumsUpdated -= OnPhotoAlbumsUpdated;
        _photoAlbum = null;
    }

    private void OnPhotoAlbumsUpdated()
    {
        _uiManager.DeferAction(() =>
        {
            if (_window is { Disposed: false })
                _window.AddOrUpdatePhotoReportTab();
        });
    }
    #endregion
}
