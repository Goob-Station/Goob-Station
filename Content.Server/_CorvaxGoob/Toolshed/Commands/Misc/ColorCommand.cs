using Content.Server.Administration;
using Content.Shared._CorvaxGoob.ColorVisuals;
using Content.Shared.Administration;
using Robust.Server.GameObjects;
using Robust.Shared.Toolshed;

namespace Content.Server._CorvaxGoob.Toolshed.Commands.Misc;

/// <summary>
/// Изменяет цвет спрайта.
/// </summary>
[ToolshedCommand, AdminCommand(AdminFlags.VarEdit)]
public sealed class ColorCommand : ToolshedCommand
{
    private AppearanceSystem? _appearance;

    [CommandImplementation("set")]
    public IEnumerable<EntityUid> Set([PipedArgument] IEnumerable<EntityUid> input, string color)
    {
        _appearance ??= GetSys<AppearanceSystem>();

        foreach (var ent in input)
        {
            EnsureComp<ColorVisualsComponent>(ent);
            _appearance.SetData(ent, ColorVisuals.Color, Color.FromHex(color));
            yield return ent;
        }
    }

    [CommandImplementation("clear")]
    public IEnumerable<EntityUid> Clear([PipedArgument] IEnumerable<EntityUid> input)
    {
        _appearance ??= GetSys<AppearanceSystem>();

        foreach (var ent in input)
        {
            RemComp<ColorVisualsComponent>(ent);
            yield return ent;
        }
    }
}
