using Robust.Client.UserInterface;

namespace Content.Client.Administration.UI.AdminCamera;

public sealed partial class AdminCameraControl
{
    private Control? _customView;
    private EntityUid? _customViewTarget;

    private void UpdateCustomView()
    {
        var target = _entManager.TryGetComponent<TransformComponent>(CurrentCamera, out var xform)
            ? xform.ParentUid
            : EntityUid.Invalid;

        if (target == _customViewTarget)
            return;

        _customViewTarget = target;
        _customView?.Orphan();

        var ev = new GetAdminCameraViewEvent();
        if (target.IsValid())
            _entManager.EventBus.RaiseLocalEvent(target, ref ev);

        _customView = ev.View;
        CameraView.Visible = _customView == null;
        if (_customView != null)
            CameraViewBox.AddChild(_customView);
    }
}

[ByRefEvent]
public record struct GetAdminCameraViewEvent(Control? View = null);
