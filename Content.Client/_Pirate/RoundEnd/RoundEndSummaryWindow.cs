// SPDX-FileCopyrightText: 2026 Corvax Team Contributors
// SPDX-FileCopyrightText: 2026 CyberLanos <cyber.lanos00@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-only

using Content.Client._Pirate.RoundEnd.PhotoAlbum;
using Content.Client.Message;
using Content.Client.Popups;
using Content.Client.Stylesheets;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Log;
using Robust.Shared.Utility;
using System.IO;
using System.Numerics;
using static Robust.Client.UserInterface.Controls.BaseButton;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd;

public sealed partial class RoundEndSummaryWindow
{
    private readonly Dictionary<int, Guid> _photoDownloadImageIds = new();
    private readonly List<(TextureButton Button, Action<ButtonEventArgs> Handler)> _photoDownloadHandlers = new();
    private readonly List<TextureRect> _photoTextureRects = new();
    private BoxContainer? _photoReportTab;
    private int _nextPhotoDownloadId;

    public void AddOrUpdatePhotoReportTab()
    {
        if (_photoReportTab != null)
            return;

        var photoTab = MakePhotoReportTab();
        if (photoTab is null)
            return;

        _photoReportTab = photoTab;
        _roundEndTabs.AddChild(photoTab);
    }

    private BoxContainer? MakePhotoReportTab()
    {
        var stationAlbumSystem = _entityManager.System<PhotoAlbumSystem>();
        if (stationAlbumSystem.Albums is null || stationAlbumSystem.Albums.Count == 0)
            return null;

        var spriteSystem = _entityManager.System<SpriteSystem>();
        ReleasePhotoResources();

        var stationAlbumTab = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Name = Loc.GetString("round-end-summary-window-photo-album-tab-title")
        };

        var stationAlbumContainerScrollbox = new ScrollContainer
        {
            VerticalExpand = true,
            Margin = new Thickness(10),
            HScrollEnabled = false,
        };

        var stationAlbumContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };

        SpriteSpecifier.Texture downloadIconTexture = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/in.svg.192dpi.png"));

        foreach (var album in stationAlbumSystem.Albums)
        {
            var gridContainer = new GridContainer();

            gridContainer.Columns = 2;
            gridContainer.HorizontalExpand = true;

            foreach (var image in album.Images)
            {
                Texture? texture = null;
                if (image.PreviewData is { Length: > 0 } previewData)
                {
                    try
                    {
                        using var stream = new MemoryStream(previewData);
                        texture = Texture.LoadFromPNGStream(stream);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Failed to load round-end photo preview for image id {image.ImageId}: {ex}");
                    }
                }

                var imageLabel = new RichTextLabel();

                if (image.CustomName is not null)
                    imageLabel.SetMessage(image.CustomName);
                else
                    imageLabel.SetMessage(Loc.GetString("round-end-summary-album-photo-no-name"));

                var imageContainer = new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    HorizontalExpand = true,
                    VerticalExpand = true
                };

                TextureRect textureRect = new TextureRect
                {
                    Margin = new Thickness(5, 10, 5, 5)
                };
                _photoTextureRects.Add(textureRect);

                TextureButton downloadButton = new TextureButton
                {
                    HorizontalAlignment = HAlignment.Right,
                    VerticalAlignment = VAlignment.Bottom
                };

                var downloadId = _nextPhotoDownloadId++;
                if (image.ImageId == Guid.Empty)
                {
                    downloadButton.Disabled = true;
                    Logger.Warning($"Round-end photo {downloadId} has an empty image id and cannot be downloaded.");
                }
                else
                {
                    _photoDownloadImageIds[downloadId] = image.ImageId;
                    Action<ButtonEventArgs> onPressed = args => DownloadButton_OnPressed(args, downloadId);
                    downloadButton.OnPressed += onPressed;
                    _photoDownloadHandlers.Add((downloadButton, onPressed));
                }

                downloadButton.Scale = new Vector2(0.5f, 0.5f);
                downloadButton.TextureNormal = spriteSystem.Frame0(downloadIconTexture);

                textureRect.Texture = texture;
                textureRect.AddChild(downloadButton);

                if (image.ImageId != Guid.Empty)
                    TryPopulateFullPhotoTexture(image.ImageId, textureRect);

                var panel = new PanelContainer
                {
                    StyleClasses = { StyleClass.BackgroundPanel },
                    ModulateSelfOverride = Color.FromHex("#1F1F23"),
                };

                imageContainer.AddChild(textureRect);
                imageContainer.AddChild(imageLabel);

                panel.AddChild(imageContainer);

                gridContainer.AddChild(panel);
            }

            var stationAlbumAuthorHeaderContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                VerticalExpand = false,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var stationAlbumAuthorHeaderPanel = new PanelContainer
            {
                StyleClasses = { StyleClass.BackgroundPanel },
                ModulateSelfOverride = Color.FromHex("#1F1F23"),
                SetSize = new Vector2(556, 30),
                HorizontalAlignment = HAlignment.Left
            };

            var stationAlbumAuthorHeaderLabel = new RichTextLabel();
            stationAlbumAuthorHeaderLabel.SetMessage(album.Title);

            stationAlbumAuthorHeaderPanel.AddChild(stationAlbumAuthorHeaderLabel);
            stationAlbumAuthorHeaderContainer.AddChild(stationAlbumAuthorHeaderPanel);

            stationAlbumContainer.AddChild(stationAlbumAuthorHeaderContainer);
            stationAlbumContainer.AddChild(gridContainer);
        }

        stationAlbumContainerScrollbox.AddChild(stationAlbumContainer);
        stationAlbumTab.AddChild(stationAlbumContainerScrollbox);

        return stationAlbumTab;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            ReleasePhotoResources();

        base.Dispose(disposing);
    }

    private void ReleasePhotoResources()
    {
        foreach (var (button, handler) in _photoDownloadHandlers)
        {
            button.OnPressed -= handler;
        }
        _photoDownloadHandlers.Clear();

        foreach (var textureRect in _photoTextureRects)
        {
            textureRect.Texture = null;
        }
        _photoTextureRects.Clear();

        _photoDownloadImageIds.Clear();
    }

    private async void TryPopulateFullPhotoTexture(Guid imageId, TextureRect textureRect)
    {
        if (!_photoTextureRects.Contains(textureRect))
            return;

        try
        {
            var stationAlbumSystem = _entityManager.System<PhotoAlbumSystem>();
            var fullImageBytes = await stationAlbumSystem.GetFullImageDataAsync(imageId);
            if (fullImageBytes is not { Length: > 0 } || !_photoTextureRects.Contains(textureRect))
                return;

            using var stream = new MemoryStream(fullImageBytes);
            textureRect.Texture = Texture.LoadFromPNGStream(stream);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to load round-end full photo for image id {imageId}: {ex}");
        }
    }

    private async void DownloadButton_OnPressed(ButtonEventArgs _, int imageId)
    {
        if (!_photoDownloadImageIds.TryGetValue(imageId, out var photoImageId))
        {
            Logger.Warning($"Round-end photo download id miss for image id {imageId}.");
            return;
        }

        (Stream fileStream, bool alreadyExisted)? file = null;

        try
        {
            var stationAlbumSystem = _entityManager.System<PhotoAlbumSystem>();
            var fullImageBytes = await stationAlbumSystem.GetFullImageDataAsync(photoImageId);
            if (fullImageBytes is not { Length: > 0 })
            {
                Logger.Warning($"Round-end photo full image fetch failed for image id {imageId}, photo id {photoImageId}.");
                _entityManager.System<PopupSystem>().PopupCursor(Loc.GetString("round-end-summary-album-photo-save-failed"));
                return;
            }

            file = await _fileDialogManager.SaveFile(new FileDialogFilters(new FileDialogFilters.Group("png")));
            if (!file.HasValue)
                return;

            await file.Value.fileStream.WriteAsync(fullImageBytes, 0, fullImageBytes.Length);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to save round-end photo for image id {imageId}: {ex}");
            _entityManager.System<PopupSystem>().PopupCursor(Loc.GetString("round-end-summary-album-photo-save-failed"));
        }
        finally
        {
            if (file.HasValue)
                await file.Value.fileStream.DisposeAsync();
        }
    }
}


