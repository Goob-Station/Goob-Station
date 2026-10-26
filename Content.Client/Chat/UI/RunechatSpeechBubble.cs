using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Shared.Chat;
using Content.Shared.IdentityManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Chat.UI;

/// <summary>
/// SS13-style runechat: outlined pixel text floating over the speaker, colored by the speaker's name.
/// </summary>
public sealed class RunechatSpeechBubble : SpeechBubble
{
    private const float RunechatMaxWidth = 200;
    private static readonly TimeSpan FadeInTime = TimeSpan.FromSeconds(0.15);
    public static readonly Color LoocColor = Color.FromHex("#48d1cc");

    private TimeSpan _spawnTime;
    private IGameTiming? _timing;

    public RunechatSpeechBubble(ChatMessage message, EntityUid senderEntity, SpeechType type)
        : base(message, senderEntity, GetStyleKey(type))
    {
    }

    private static string GetStyleKey(SpeechType type)
    {
        return type switch
        {
            SpeechType.Emote => "emote",
            SpeechType.Whisper => "whisper",
            SpeechType.Looc => "looc",
            _ => "say",
        };
    }

    protected override Control BuildBubble(ChatMessage message, string speechStyleClass, Color? fontColor = null)
    {
        _timing = IoCManager.Resolve<IGameTiming>();
        _spawnTime = _timing.RealTime;

        var entMan = IoCManager.Resolve<IEntityManager>();
        var sender = entMan.GetEntity(message.SenderEntity);
        var type = speechStyleClass switch
        {
            "emote" => SpeechType.Emote,
            "whisper" => SpeechType.Whisper,
            "looc" => SpeechType.Looc,
            _ => SpeechType.Say,
        };

        var header = FormattedMessage.RemoveMarkupPermissive(SharedChatSystem.GetStringInsideTag(message, "BubbleHeader")).Trim();
        if (header.Length == 0 && entMan.EntityExists(sender))
            header = Identity.Name(sender, entMan);

        string text;
        var color = GetNameColor(header);
        var loud = false;

        switch (type)
        {
            case SpeechType.Emote:
                text = "*" + FormattedMessage.RemoveMarkupPermissive(message.Message).Trim() + "*";
                break;
            case SpeechType.Looc:
                text = FormattedMessage.RemoveMarkupPermissive(message.Message);
                color = LoocColor;
                break;
            default:
                var content = SharedChatSystem.GetStringInsideTag(message, "BubbleContent");
                if (content.Length == 0)
                    content = message.Message;

                if (content.Contains("[bold]"))
                    loud = true;

                text = FormattedMessage.RemoveMarkupPermissive(content);
                break;
        }

        var cfg = IoCManager.Resolve<IConfigurationManager>();
        return CreateLabel(text,
            color,
            type == SpeechType.Whisper,
            loud,
            cfg.GetCVar(GoobCVars.ChatRunechatFontSize),
            cfg.GetCVar(GoobCVars.ChatRunechatOutlineScale));
    }

    public static RichTextLabel CreateLabel(string text, Color color, bool whisper, bool loud, int fontSize, float outlineScale)
    {
        fontSize = Math.Clamp(fontSize, RunechatStyle.MinFontSize, RunechatStyle.MaxFontSize);
        var size = loud && !whisper ? RunechatStyle.GetLoudSize(fontSize) : fontSize;

        var msg = new FormattedMessage();
        msg.AddMarkupPermissive($"[color={color.ToHex()}]{FormattedMessage.EscapeText(text.Trim())}[/color]");

        var outline = RunechatStyle.GetOutline(size, outlineScale);
        var pad = MathF.Ceiling(outline);
        var label = new RichTextLabel
        {
            MaxWidth = RunechatMaxWidth,
            OutlineThicknessOverride = outline,
            OutlineColorOverride = Color.Black,
            Margin = new Thickness(pad, pad, pad, pad + 2),
            Stylesheet = RunechatStyle.GetStylesheet(whisper ? RunechatStyle.WhisperFontPath : RunechatStyle.FontPath, size),
        };
        label.SetMessage(msg, AllowedTags);

        return label;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        var pixelPosition = (Position * UIScale).Rounded() / UIScale;
        LayoutContainer.SetPosition(this, pixelPosition);

        if (_timing == null)
            return;

        var age = (float) ((_timing.RealTime - _spawnTime).TotalSeconds / FadeInTime.TotalSeconds);
        if (age < 1f)
            Modulate = Modulate.WithAlpha(Modulate.A * Math.Clamp(age, 0f, 1f));
    }

    public static Color GetNameColor(string name)
    {
        var hash = 2166136261u;
        foreach (var c in name)
        {
            hash ^= c;
            hash *= 16777619u;
        }

        var hue = (hash % 360) / 360f;
        var saturation = 0.55f + ((hash >> 9) % 30) / 100f;
        var lightness = 0.65f + ((hash >> 17) % 15) / 100f;
        return Color.FromHsl(new Vector4(hue, saturation, lightness, 1f));
    }
}
