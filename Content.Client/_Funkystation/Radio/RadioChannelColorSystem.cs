using System.Text;
using Content.Goobstation.Common.CCVar;
using Content.Shared.Radio;
using Robust.Shared.Configuration;

namespace Content.Client._Funkystation.Radio;

/// <summary>
/// Resolves the color a radio channel should be drawn with, honoring the player's custom channel colors.
/// </summary>
public sealed class RadioChannelColorSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;

    private bool _useCustom;
    private readonly Dictionary<string, Color> _overrides = new();

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.UseCustomRadioChannelColors, value => _useCustom = value, true);
        Subs.CVar(_cfg, GoobCVars.RadioChannelColors, OnColorsChanged, true);
    }

    public Color GetColor(string channelId)
    {
        if (_useCustom && _overrides.TryGetValue(channelId, out var custom))
            return custom;

        return ProtoMan.TryIndex<RadioChannelPrototype>(channelId, out var proto) ? proto.Color : Color.White;
    }

    private void OnColorsChanged(string value)
    {
        _overrides.Clear();
        foreach (var (channel, color) in Parse(value))
        {
            _overrides[channel] = color;
        }
    }

    public static Dictionary<string, Color> Parse(string value)
    {
        var result = new Dictionary<string, Color>();
        foreach (var entry in value.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var split = entry.Split('=', 2);
            if (split.Length == 2 && Color.TryFromHex(split[1].Trim(), out var color))
                result[split[0].Trim()] = color;
        }

        return result;
    }

    public static string Serialize(IEnumerable<KeyValuePair<string, Color>> colors)
    {
        var builder = new StringBuilder();
        foreach (var (channel, color) in colors)
        {
            if (builder.Length > 0)
                builder.Append(';');
            builder.Append(channel).Append('=').Append(color.ToHex());
        }

        return builder.ToString();
    }
}
