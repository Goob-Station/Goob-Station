using Content.Goobstation.Shared.Twitch;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Twitch;

public sealed class TwitchBitsToastSystem : EntitySystem
{
    private static readonly TimeSpan ToastLifetime = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan FadeDuration = TimeSpan.FromSeconds(1);
    private const int MaximumToasts = 4;

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    private readonly List<ToastEntry> _toasts = [];
    private BoxContainer? _stack;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<TwitchBitsToastEvent>(OnToast);

        _stack = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            MinWidth = 300,
            MouseFilter = Control.MouseFilterMode.Ignore,
        };
        _ui.RootControl.AddChild(_stack);
        LayoutContainer.SetAnchorAndMarginPreset(_stack, LayoutContainer.LayoutPreset.TopRight, 20);
        LayoutContainer.SetMarginTop(_stack, 80);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _stack?.Orphan();
        _stack = null;
        _toasts.Clear();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        for (var i = _toasts.Count - 1; i >= 0; i--)
        {
            var toast = _toasts[i];
            var age = now - toast.CreatedAt;
            if (age >= ToastLifetime)
            {
                RemoveToast(i);
                continue;
            }

            var fadeStart = ToastLifetime - FadeDuration;
            if (age > fadeStart)
            {
                var alpha = 1f - (float) ((age - fadeStart) / FadeDuration);
                toast.Control.Modulate = Color.White.WithAlpha(alpha);
            }
        }
    }

    private void OnToast(TwitchBitsToastEvent message)
    {
        if (_stack == null || string.IsNullOrWhiteSpace(message.Message))
            return;

        while (_toasts.Count >= MaximumToasts)
            RemoveToast(0);

        var style = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#24242bee"),
            BorderColor = Color.FromHex("#a88b5e"),
            BorderThickness = new Thickness(2),
            ContentMarginLeftOverride = 12,
            ContentMarginTopOverride = 9,
            ContentMarginRightOverride = 12,
            ContentMarginBottomOverride = 9,
        };
        var text = new Label
        {
            Text = message.Message,
            FontColorOverride = Color.FromHex("#e5e5e5"),
            MouseFilter = Control.MouseFilterMode.Ignore,
        };
        var panel = new PanelContainer
        {
            PanelOverride = style,
            MouseFilter = Control.MouseFilterMode.Ignore,
            Children = { text },
        };

        _stack.AddChild(panel);
        _toasts.Add(new ToastEntry(panel, _timing.CurTime));
    }

    private void RemoveToast(int index)
    {
        var toast = _toasts[index];
        toast.Control.Orphan();
        _toasts.RemoveAt(index);
    }

    private sealed record ToastEntry(Control Control, TimeSpan CreatedAt);
}
