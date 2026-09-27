using System.Numerics;
using Content.Client.UserInterface.Systems.Chat.Controls;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoiceChat.UI;

public sealed class ChatVoiceControls : IChatVoiceControls
{
    public IEnumerable<Control> CreateControls()
    {
        yield return new VoiceMicButton();
        yield return new VoiceDeafenButton();
    }
}

public abstract class VoiceIconButton : Button
{
    [Dependency] protected readonly IConfigurationManager Cfg = default!;
    [Dependency] protected readonly VoiceChatManager Voice = default!;
    [Dependency] protected readonly IEntitySystemManager Systems = default!;

    protected static readonly Color OfflineColor = Color.FromHex("#6A6C7A");
    protected static readonly Color IdleColor = Color.FromHex("#C8C8D0");
    protected static readonly Color LiveColor = Color.FromHex("#5CD65C");
    protected static readonly Color WarningColor = Color.FromHex("#FF9F43");
    protected static readonly Color MutedColor = Color.FromHex("#E04545");

    protected Color IconColor = OfflineColor;
    protected bool Slashed;

    protected VoiceIconButton()
    {
        IoCManager.InjectDependencies(this);
        MinWidth = 30;
        StyleClasses.Add(ChannelSelectorItemButton.StyleClassChatSelectorOptionButton);
        Cfg.OnValueChanged(GoobCVars.VoiceChatEnabled, OnEnabledChanged, true);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            Cfg.UnsubValueChanged(GoobCVars.VoiceChatEnabled, OnEnabledChanged);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        UpdateState();
    }

    protected abstract void UpdateState();

    protected abstract void DrawIcon(DrawingHandleScreen handle, Vector2 center, float scale);

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var scale = UIScale;
        var center = new Vector2(PixelWidth / 2f, PixelHeight / 2f);
        DrawIcon(handle, center, scale);

        if (!Slashed)
            return;

        var from = center + new Vector2(-7f, -8f) * scale;
        var to = center + new Vector2(7f, 8f) * scale;
        for (var offset = -1; offset <= 1; offset++)
        {
            var shift = new Vector2(offset, 0f);
            handle.DrawLine(from + shift, to + shift, MutedColor);
        }
    }

    protected static void DrawThickLine(DrawingHandleScreen handle, Vector2 from, Vector2 to, Color color)
    {
        handle.DrawLine(from, to, color);
        handle.DrawLine(from + Vector2.UnitX, to + Vector2.UnitX, color);
    }

    protected static void DrawArc(DrawingHandleScreen handle, Vector2 center, float radius, float start, float end, Color color)
    {
        const int segments = 12;
        var previous = center + new Vector2(MathF.Cos(start), MathF.Sin(start)) * radius;
        for (var i = 1; i <= segments; i++)
        {
            var angle = start + (end - start) * i / segments;
            var next = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            DrawThickLine(handle, previous, next, color);
            previous = next;
        }
    }

    private void OnEnabledChanged(bool enabled)
    {
        Visible = enabled;
    }
}

public sealed class VoiceMicButton : VoiceIconButton
{
    public VoiceMicButton()
    {
        OnPressed += _ => OnClick();
    }

    protected override void UpdateState()
    {
        string tooltip;
        if (!Voice.WebConnected)
        {
            IconColor = OfflineColor;
            Slashed = false;
            tooltip = "voice-mic-offline";
        }
        else if (Voice.Deafened)
        {
            IconColor = MutedColor;
            Slashed = true;
            tooltip = "voice-mic-deafened";
        }
        else if (Voice.MicMuted)
        {
            IconColor = MutedColor;
            Slashed = true;
            tooltip = "voice-mic-muted";
        }
        else if (Systems.TryGetEntitySystem<VoiceChatSystem>(out var system) && system.Self.Activity > 0f)
        {
            var blocked = (system.Self.Flags & VoiceSelfFlags.Blocked) != 0;
            IconColor = blocked ? WarningColor : LiveColor;
            Slashed = false;
            tooltip = blocked ? "voice-mic-blocked" : "voice-mic-live";
        }
        else
        {
            IconColor = IdleColor;
            Slashed = false;
            tooltip = "voice-mic-ready";
        }

        var text = Loc.GetString(tooltip);
        if (ToolTip != text)
            ToolTip = text;
    }

    protected override void DrawIcon(DrawingHandleScreen handle, Vector2 center, float scale)
    {
        var top = center + new Vector2(0f, -5f) * scale;
        var bottom = center + new Vector2(0f, 0f) * scale;
        var radius = 2.5f * scale;

        handle.DrawRect(new UIBox2(top.X - radius, top.Y, top.X + radius, bottom.Y), IconColor);
        handle.DrawCircle(top, radius, IconColor);
        handle.DrawCircle(bottom, radius, IconColor);

        DrawArc(handle, bottom, 5f * scale, 0f, MathF.PI, IconColor);
        DrawThickLine(handle, center + new Vector2(-5f, -2f) * scale, center + new Vector2(-5f, 0f) * scale, IconColor);
        DrawThickLine(handle, center + new Vector2(5f, -2f) * scale, center + new Vector2(5f, 0f) * scale, IconColor);
        DrawThickLine(handle, center + new Vector2(0f, 5f) * scale, center + new Vector2(0f, 8f) * scale, IconColor);
        DrawThickLine(handle, center + new Vector2(-3f, 8f) * scale, center + new Vector2(3f, 8f) * scale, IconColor);
    }

    private void OnClick()
    {
        if (!Voice.WebConnected)
        {
            UserInterfaceManager.GetUIController<VoiceChatGuideUIController>().Open();
            return;
        }

        if (Voice.Deafened)
        {
            Voice.SetDeafened(false);
            Voice.SetMicMuted(false);
            return;
        }

        Voice.SetMicMuted(!Voice.MicMuted);
    }
}

public sealed class VoiceDeafenButton : VoiceIconButton
{
    public VoiceDeafenButton()
    {
        OnPressed += _ => Voice.SetDeafened(!Voice.Deafened);
    }

    protected override void UpdateState()
    {
        IconColor = Voice.Deafened ? MutedColor : IdleColor;
        Slashed = Voice.Deafened;

        var text = Loc.GetString(Voice.Deafened ? "voice-deafen-on" : "voice-deafen-off");
        if (ToolTip != text)
            ToolTip = text;
    }

    protected override void DrawIcon(DrawingHandleScreen handle, Vector2 center, float scale)
    {
        var band = center + new Vector2(0f, 1f) * scale;
        DrawArc(handle, band, 7f * scale, MathF.PI, MathF.PI * 2f, IconColor);

        handle.DrawRect(new UIBox2(
            center + new Vector2(-8.5f, 0f) * scale,
            center + new Vector2(-5f, 7f) * scale), IconColor);
        handle.DrawRect(new UIBox2(
            center + new Vector2(5f, 0f) * scale,
            center + new Vector2(8.5f, 7f) * scale), IconColor);
    }
}
