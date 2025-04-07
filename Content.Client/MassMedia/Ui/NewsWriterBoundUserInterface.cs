// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <milonpl.git@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;
using Content.Shared.MassMedia.Systems;
using Content.Shared.MassMedia.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Client.MassMedia.Ui;

[UsedImplicitly]
public sealed class NewsWriterBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NewsWriterMenu? _menu;

    public NewsWriterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<NewsWriterMenu>();

        _menu.ArticleEditorPanel.PublishButtonPressed += OnPublishButtonPressed;
        _menu.DeleteButtonPressed += OnDeleteButtonPressed;

        _menu.CreateButtonPressed += OnCreateButtonPressed;
        _menu.ArticleEditorPanel.ArticleDraftUpdated += OnArticleDraftUpdated;

        SendMessage(new NewsWriterArticlesRequestMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not NewsWriterBoundUserInterfaceState cast)
            return;

        _menu?.UpdateUI(cast.Articles, cast.PublishEnabled, cast.NextPublish, cast.DraftTitle, cast.DraftContent);
    }

    private void OnPublishButtonPressed()
    {
        var title = _menu?.ArticleEditorPanel.TitleField.Text.Trim() ?? "";
        if (_menu == null || title.Length == 0)
            return;

        var stringContent = Rope.Collapse(_menu.ArticleEditorPanel.ContentField.TextRope).Trim();

        if (stringContent.Length == 0)
            return;

        var name = title.Length <= SharedNewsSystem.MaxTitleLength
            ? title
            : $"{title[..(SharedNewsSystem.MaxTitleLength - 3)]}...";

        var content = stringContent.Length <= SharedNewsSystem.MaxContentLength
            ? stringContent
            : $"{stringContent[..(SharedNewsSystem.MaxContentLength - 3)]}...";


        SendMessage(new NewsWriterPublishMessage(name, content));
    }

    private void OnDeleteButtonPressed(int articleNum)
    {
        if (_menu == null)
            return;

        SendMessage(new NewsWriterDeleteMessage(articleNum));
    }

    private void OnCreateButtonPressed()
    {
        SendMessage(new NewsWriterRequestDraftMessage());
    }

    private void OnArticleDraftUpdated(string title, string content)
    {
        SendMessage(new NewsWriterSaveDraftMessage(title, content));
    }
}