using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Pirate.Plumbing.UI;

public sealed class ClickableBeakerBarChart : ContainerButton
{
    private static readonly Color _idleBackgroundColor = Color.FromHex("#25252A");
    private static readonly Color _chartBackgroundColor = new(0.1f, 0.1f, 0.1f);
    private static readonly Color _hoverBackgroundColor = Color.FromHex("#303038");
    private static readonly Color _pressedBackgroundColor = Color.FromHex("#1E1E24");

    private readonly ProgressBar _chart;
    private readonly Label _label;

    public event Action<string>? OnChartPressed;

    public string ReagentId { get; set; } = string.Empty;

    public float Capacity
    {
        get => _chart.MaxValue;
        set => _chart.MaxValue = Math.Max(value, 1f);
    }

    public ClickableBeakerBarChart()
    {
        HorizontalExpand = true;
        MouseFilter = MouseFilterMode.Stop;
        ToolTip = string.Empty;
        OnPressed += _ => HandlePressed();

        var chartContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(4),
        };

        _label = new Label
        {
            HorizontalExpand = true,
            ClipText = true,
        };

        _chart = new ProgressBar
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            MouseFilter = MouseFilterMode.Ignore,
            MinValue = 0,
            MaxValue = 1,
            BackgroundStyleBoxOverride = new StyleBoxFlat(_chartBackgroundColor),
            ForegroundStyleBoxOverride = new StyleBoxFlat(Color.White),
        };

        chartContainer.AddChild(_label);
        chartContainer.AddChild(_chart);
        AddChild(chartContainer);
        UpdateButtonStyle();
    }

    public void Clear()
    {
        _chart.Value = 0;
        _label.Text = string.Empty;
        ToolTip = string.Empty;
    }

    public void SetEntry(
        string uid,
        string label,
        float amount,
        Color color,
        Color? textColor = null,
        string? tooltip = null)
    {
        ToolTip = tooltip;
        _label.Text = label;
        _label.FontColorOverride = textColor;
        _chart.Value = amount;
        _chart.ForegroundStyleBoxOverride = new StyleBoxFlat(color);
    }

    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();
        UpdateButtonStyle();
    }

    private void HandlePressed()
    {
        if (string.IsNullOrEmpty(ReagentId))
            return;

        OnChartPressed?.Invoke(ReagentId);
    }

    private void UpdateButtonStyle()
    {
        var backgroundColor = DrawMode switch
        {
            DrawModeEnum.Pressed => _pressedBackgroundColor,
            DrawModeEnum.Hover => _hoverBackgroundColor,
            _ => _idleBackgroundColor,
        };

        StyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = backgroundColor,
            BorderColor = Color.Transparent,
            BorderThickness = new Thickness(0),
            ContentMarginLeftOverride = 0,
            ContentMarginTopOverride = 0,
            ContentMarginRightOverride = 0,
            ContentMarginBottomOverride = 0,
        };
    }
}
