// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Controls;
using Content.Shared.BloodCult;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BloodCult;

[UsedImplicitly]
public sealed class TeleportRuneNameBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private readonly FancyWindow _window = new();

    protected override void Open()
    {
        base.Open();

        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };

        container.AddChild(new Label
        {
            Text = Loc.GetString("cult-teleport-name-title"),
        });

        var lineEdit = new LineEdit
        {
            HorizontalExpand = true,
        };

        var button = new Button
        {
            Text = Loc.GetString("cult-teleport-name-accept"),
        };

        button.OnButtonUp += _ =>
        {
            SendMessage(new TeleportRuneNameSelectedMessage(lineEdit.Text));
            Close();
        };

        container.AddChild(lineEdit);
        container.AddChild(button);
        _window.AddChild(container);
        _window.OpenCentered();
        _window.OnClose += Close;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window.Close();
    }
}
