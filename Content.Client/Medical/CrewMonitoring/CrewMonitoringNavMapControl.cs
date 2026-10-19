// SPDX-License-Identifier: MIT

using Content.Client.Pinpointer.UI;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client.Medical.CrewMonitoring;

public sealed partial class CrewMonitoringNavMapControl : NavMapControl
{
    public NetEntity? Focus;
    public Dictionary<NetEntity, string> LocalizedNames = new();

    private Label _trackedEntityLabel;
    private PanelContainer _trackedEntityPanel;

    public Button? WarpButton;
    public event Action<NetEntity>? OnWarpPressed;

    public CrewMonitoringNavMapControl() : base()
    {
        WallColor = new Color(192, 122, 196);
        TileColor = new(71, 42, 72);
        BackgroundColor = Color.FromSrgb(TileColor.WithAlpha(BackgroundOpacity));

        var panelBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Margin = new Thickness(8f, 6f),
        };

        _trackedEntityLabel = new Label
        {
            Margin = new Thickness(4f, 2f),
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            Modulate = Color.White,
        };

        WarpButton = new Button
        {
            Text = Loc.GetString("crew-monitoring-ui-warp-button"),
            Margin = new Thickness(8f, 2f, 4f, 2f),
            VerticalAlignment = VAlignment.Center,
            Visible = false,
        };
        WarpButton.OnPressed += _ =>
        {
            if (Focus != null)
                OnWarpPressed?.Invoke(Focus.Value);
        };

        panelBox.AddChild(_trackedEntityLabel);
        panelBox.AddChild(WarpButton);

        _trackedEntityPanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = BackgroundColor,
            },

            Margin = new Thickness(5f, 10f),
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Bottom,
            Visible = false,
        };

        _trackedEntityPanel.AddChild(panelBox);
        this.AddChild(_trackedEntityPanel);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (Focus == null)
        {
            _trackedEntityLabel.Text = string.Empty;
            _trackedEntityPanel.Visible = false;

            return;
        }

        foreach ((var netEntity, var blip) in TrackedEntities)
        {
            if (netEntity != Focus)
                continue;

            if (!LocalizedNames.TryGetValue(netEntity, out var name))
                name = Loc.GetString("navmap-unknown-entity");

            var message = name + "\n" + Loc.GetString("navmap-location",
                ("x", MathF.Round(blip.Coordinates.X)),
                ("y", MathF.Round(blip.Coordinates.Y)));

            _trackedEntityLabel.Text = message;
            _trackedEntityPanel.Visible = true;
            if (WarpButton != null)
                WarpButton.Visible = OnWarpPressed != null;

            return;
        }

        _trackedEntityLabel.Text = string.Empty;
        _trackedEntityPanel.Visible = false;
    }
}