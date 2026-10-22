using Content.Goobstation.Common.CCVar;
using Robust.Shared.Configuration;

namespace Content.Client._Funkystation.Outline;

/// <summary>
/// Provides the client's interaction outline colors, either their custom colors or the defaults.
/// </summary>
public sealed class InteractionOutlineColorSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;

    private Color _validColor;
    private Color _invalidColor;
    private bool _useCustomColors;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.ValidInteractionOutlineColor, _ => Refresh());
        Subs.CVar(_cfg, GoobCVars.InvalidInteractionOutlineColor, _ => Refresh());
        Subs.CVar(_cfg, GoobCVars.UseCustomInteractionOutlineColors, value =>
        {
            _useCustomColors = value;
            Refresh();
        }, true);
    }

    public Color GetOutlineColor(bool inRange)
    {
        return inRange ? _validColor : _invalidColor;
    }

    private void Refresh()
    {
        _validColor = GetColor(GoobCVars.ValidInteractionOutlineColor);
        _invalidColor = GetColor(GoobCVars.InvalidInteractionOutlineColor);
    }

    private Color GetColor(CVarDef<string> cvar)
    {
        var defaultColor = Color.FromHex(cvar.DefaultValue);
        if (!_useCustomColors)
            return defaultColor;

        var value = _cfg.GetCVar(cvar);
        if (Color.TryFromHex(value, out var color))
            return color;

        Log.Warning($"{cvar.Name} is set to an invalid color (\"{value}\"), using the default.");
        return defaultColor;
    }
}
