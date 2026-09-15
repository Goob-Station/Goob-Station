// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Shared._Oskarrr.EngineerJockey;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Maths;

namespace Content.Client._Oskarrr.EngineerJockey;

[UsedImplicitly]
public sealed class EngineerConsoleBoundUserInterface : BoundUserInterface
{
    private EngineerConsoleWindow? _window;

    public EngineerConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        if (_window != null)
        {
            _window.OnClose -= Close;
            _window.Orphan();
            _window = null;
        }
        _window = new EngineerConsoleWindow();
        _window.OnClose += Close;
        _window.OnAwakenPressed += netEnt => SendMessage(new EngineerConsoleAwakenMessage(netEnt));

        if (State is EngineerConsoleBuiState cast)
            _window.UpdateState(cast);

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is EngineerConsoleBuiState cast)
            _window?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && _window != null)
        {
            _window.OnClose -= Close;
            _window.Orphan();
            _window = null;
        }
    }
}
public sealed class EngineerConsoleWindow : DefaultWindow
{
    private readonly BoxContainer _list;
    private readonly Label _status;

    public event Action<NetEntity>? OnAwakenPressed;

    public EngineerConsoleWindow()
    {
        Title = Loc.GetString("engineer-console-title");
        SetSize = new Vector2(440, 380);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(8),
        };

        root.AddChild(new Label
        {
            Text = Loc.GetString("engineer-console-risk"),
            HorizontalExpand = true,
        });

        _status = new Label
        {
            Text = Loc.GetString("engineer-console-status-scanning"),
            Margin = new Thickness(0, 6, 0, 6),
        };
        root.AddChild(_status);

        _list = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            VerticalExpand = true,
            HorizontalExpand = true,
        };
        root.AddChild(new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            Children = { _list },
        });

        Contents.AddChild(root);
    }

    public void UpdateState(EngineerConsoleBuiState state)
    {
        _list.RemoveAllChildren();

        var occupied = state.Pods.Count(p => p.Occupied);
        _status.Text = Loc.GetString("engineer-console-status-count", ("count", occupied));

        if (state.Pods.Count == 0)
        {
            _list.AddChild(new Label { Text = Loc.GetString("engineer-console-none") });
            return;
        }

        foreach (var entry in state.Pods)
        {
            var status = entry.Occupied
                ? Loc.GetString("engineer-console-status-occupied")
                : Loc.GetString("engineer-console-status-empty");

            var button = new Button
            {
                Text = Loc.GetString("engineer-console-pod-button", ("name", entry.Name), ("status", status)),
                Disabled = !entry.Occupied,
                HorizontalExpand = true,
                Margin = new Thickness(0, 0, 0, 4),
            };

            var pod = entry.Pod;
            button.OnPressed += _ => OnAwakenPressed?.Invoke(pod);
            _list.AddChild(button);
        }
    }
}
