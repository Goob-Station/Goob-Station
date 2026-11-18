using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSpectatorControl : Control
{
    [Dependency] private IEntityManager _entManager = default!;

    private readonly NullspaceSystem _system;
    private NullspaceRenderer? _renderer;
    private readonly NullspaceFrame _frame = new();
    private readonly NullspaceTracks _tracks = new();

    public EntityUid? Target;

    public NullspaceSpectatorControl()
    {
        IoCManager.InjectDependencies(this);
        _system = _entManager.System<NullspaceSystem>();
        MinSize = new(320f, 200f);
        HorizontalExpand = true;
        VerticalExpand = true;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (Target is { } target)
            _system.FillSpectatorFrame(_frame, target, _tracks);
        else
            _frame.Maze = null;
    }

    protected override void Draw(IRenderHandle renderHandle)
    {
        renderHandle.DrawingHandleScreen.DrawRect(PixelSizeBox, Color.Black);
        _renderer ??= _system.CreateRenderer();
        _renderer.Draw(renderHandle, PixelSize, _frame);
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();
        _renderer?.Dispose();
        _renderer = null;
    }
}
