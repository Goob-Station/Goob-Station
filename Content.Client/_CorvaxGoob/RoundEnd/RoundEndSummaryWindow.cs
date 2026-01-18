using Content.Client._CorvaxGoob.RoundEnd.PhotoAlbum;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using System.IO;
using System.Numerics;
using static Robust.Client.UserInterface.Controls.BaseButton;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd;

public sealed partial class RoundEndSummaryWindow
{
    private BoxContainer? MakePhotoReportTab()
    {
        var stationAlbumSystem = _entityManager.System<PhotoAlbumSystem>();
        var spriteSystem = _entityManager.System<SpriteSystem>();

        var stationAlbumTab = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Name = Loc.GetString("round-end-summary-window-photo-album-tab-title")
        };

        stationAlbumTab.RemoveAllChildren();

        if (stationAlbumSystem.Albums is null || stationAlbumSystem.Albums.Count == 0)
            return null;

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
                MemoryStream stream = new MemoryStream(image.Key);

                var imageLabel = new RichTextLabel();

                if (image.Value is not null)
                    imageLabel.SetMessage(image.Value);
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

                TextureButton downloadButton = new TextureButton
                {
                    HorizontalAlignment = HAlignment.Right,
                    VerticalAlignment = VAlignment.Bottom
                };

                downloadButton.OnPressed += _ => DownloadButton_OnPressed(_, image.Key);

                downloadButton.Scale = new Vector2(0.5f, 0.5f);
                downloadButton.TextureNormal = spriteSystem.Frame0(downloadIconTexture);

                textureRect.Texture = Texture.LoadFromPNGStream(stream);
                textureRect.AddChild(downloadButton);

                var panel = new PanelContainer
                {
                    StyleClasses = { StyleNano.StyleClassBackgroundBaseDark },
                };

                imageContainer.AddChild(textureRect);
                imageContainer.AddChild(imageLabel);

                panel.AddChild(imageContainer);

                gridContainer.AddChild(panel);
            }

            stationAlbumContainer.AddChild(gridContainer);

            var stationAlbumAuthorHeaderContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                VerticalExpand = true,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var stationAlbumAuthorHeaderPanel = new PanelContainer
            {
                StyleClasses = { StyleNano.StyleClassBackgroundBaseDark },
                SetSize = new Vector2(556, 30),
                HorizontalAlignment = HAlignment.Left
            };

            var stationAlbumAuthorHeaderLabel = new RichTextLabel();

            string authorName = album.AuthorName == null ? Loc.GetString("round-end-summary-album-photo-no-author-name") : album.AuthorName;
            string authorCKey = album.AuthorCkey == null ? Loc.GetString("round-end-summary-album-photo-no-author-ckey") : album.AuthorCkey;

            stationAlbumAuthorHeaderLabel.SetMarkup(Loc.GetString("round-end-summary-album-photo-author", ("authorName", authorName), ("authorCKey", authorCKey)));

            stationAlbumAuthorHeaderPanel.AddChild(stationAlbumAuthorHeaderLabel);
            stationAlbumAuthorHeaderContainer.AddChild(stationAlbumAuthorHeaderPanel);

            stationAlbumContainer.AddChild(stationAlbumAuthorHeaderContainer);
        }

        stationAlbumContainerScrollbox.AddChild(stationAlbumContainer);
        stationAlbumTab.AddChild(stationAlbumContainerScrollbox);

        stationAlbumSystem.ClearImagesData();

        return stationAlbumTab;
    }

    private async void DownloadButton_OnPressed(ButtonEventArgs _, byte[] data)
    {
        var file = await _fileDialogManager.SaveFile(new FileDialogFilters(new FileDialogFilters.Group("png")));

        if (!file.HasValue)
            return;

        try
        {
            await file.Value.fileStream.WriteAsync(data, 0, data.Length);
        }
        finally
        {
            await file.Value.fileStream.DisposeAsync();
        }
    }
}
