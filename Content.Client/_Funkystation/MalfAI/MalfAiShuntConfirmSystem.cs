// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._Funkystation.MalfAI;

/// <summary>
/// Small confirmation dialog shown when shunting to an APC would abort the active doomsday protocol.
/// </summary>
public sealed class MalfAiShuntConfirmSystem : EntitySystem
{
    private DefaultWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<MalfAiShuntConfirmRequestEvent>(OnConfirmRequest);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _window?.Close();
        _window = null;
    }

    private void OnConfirmRequest(MalfAiShuntConfirmRequestEvent ev)
    {
        _window?.Close();

        var window = new DefaultWindow
        {
            Title = Loc.GetString("malfai-shunt-doomsday-confirm-title"),
            MinWidth = 360,
        };

        var confirmButton = new Button { Text = Loc.GetString("malfai-shunt-doomsday-confirm-yes") };
        var cancelButton = new Button { Text = Loc.GetString("malfai-shunt-doomsday-confirm-no") };

        confirmButton.OnPressed += _ =>
        {
            RaiseNetworkEvent(new MalfAiShuntConfirmResponseEvent(ev.Apc));
            window.Close();
        };
        cancelButton.OnPressed += _ => window.Close();

        var buttons = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalAlignment = Control.HAlignment.Center,
            SeparationOverride = 8,
        };
        buttons.AddChild(confirmButton);
        buttons.AddChild(cancelButton);

        var layout = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(8),
            SeparationOverride = 8,
        };
        layout.AddChild(new Label
        {
            Text = Loc.GetString("malfai-shunt-doomsday-confirm-text"),
        });
        layout.AddChild(buttons);

        window.Contents.AddChild(layout);
        window.OnClose += () => _window = null;

        _window = window;
        window.OpenCentered();
    }
}
