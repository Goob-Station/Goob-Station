using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;

namespace Content.Goobstation.Client.Nullspace;

public sealed class NullspaceScreen : Control
{
    private readonly NullspaceSystem _system;
    private readonly NullspaceRenderer _renderer;
    private readonly NullspaceFrame _frame;

    public NullspaceScreen(NullspaceSystem system, NullspaceRenderer renderer, NullspaceFrame frame)
    {
        _system = system;
        _renderer = renderer;
        _frame = frame;
        MouseFilter = MouseFilterMode.Stop;
        HorizontalExpand = true;
        VerticalExpand = true;
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);
        _system.AddLook(args.Relative * UIScale);
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _system.Capture();
        args.Handle();
    }

    protected override void Draw(IRenderHandle renderHandle)
    {
        _renderer.Draw(renderHandle, PixelSize, _frame);
    }
}
