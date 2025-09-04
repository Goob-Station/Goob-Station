using Content.Client.UserInterface.Controls;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Client._White.UI.Controls;

[Virtual]
public class TrackedRadialMenu : RadialMenu
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;

    private EntityUid _trackedEntity;

    public TrackedRadialMenu()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        UpdatePosition();
    }

    public void Track(EntityUid trackedEntity)
    {
        _trackedEntity = trackedEntity;

        if (!_entManager.EntityExists(_trackedEntity))
        {
            Close();
            return;
        }

        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (!_entManager.TryGetComponent(_trackedEntity, out TransformComponent? xform)
            || !xform.Coordinates.IsValid(_entManager))
        {
            Close();
            return;
        }

        var coords = _entManager.System<SpriteSystem>().GetSpriteScreenCoordinates((_trackedEntity, null, xform));

        if (!coords.IsValid)
        {
            Close();
            return;
        }

        OpenScreenAt(coords.Position, _clyde);
    }
}
