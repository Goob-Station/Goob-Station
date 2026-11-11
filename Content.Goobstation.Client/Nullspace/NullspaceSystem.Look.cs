using System.Numerics;
using Content.Goobstation.Shared.Nullspace;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSystem
{
    private const float MaxPitch = 1.45f;
    private const float MaxLookJump = 400f;
    private static readonly TimeSpan LookSendInterval = TimeSpan.FromSeconds(1.0 / 60.0);

    private Vector2 _lookDelta;
    private bool _captured;
    private bool _relative;
    private bool _startSent;
    private float _sensitivity;
    private float _yaw;
    private float _pitch;
    private float _sentYaw = float.NaN;
    private float _sentPitch = float.NaN;
    private TimeSpan _nextLookSend;

    public void AddLook(Vector2 delta)
    {
        if (_captured)
            _lookDelta += delta;
    }

    public void Capture()
    {
        if (_viewer is not { } uid)
            return;

        _captured = true;
        if (_startSent || !TryGetMap(uid, out var map) || map.Comp.StartTime != null)
            return;

        _startSent = true;
        RaiseNetworkEvent(new NullspaceStartEvent());
    }

    private void UpdateCapture()
    {
        if (_captured && (!_clyde.MainWindow.IsFocused || WindowOpen()))
            _captured = false;

        SetRelative(_captured);

        if (_screen == null)
            return;

        if (_captured)
            _ui.ControlFocused = _screen;
        else if (_ui.ControlFocused == _screen)
            _ui.ControlFocused = null;
    }

    private void SetRelative(bool relative)
    {
        if (_relative == relative)
            return;

        _relative = relative;
        _lookDelta = Vector2.Zero;
        _clyde.MainWindow.SetRelativeMouseMode(relative);
    }

    private bool WindowOpen()
    {
        foreach (var child in _ui.WindowRoot.Children)
        {
            if (child is BaseWindow { IsOpen: true } && child.Visible)
                return true;
        }

        foreach (var child in _ui.ModalRoot.Children)
        {
            if (child.Visible)
                return true;
        }

        return false;
    }

    private void UpdateLook(Entity<NullspacePlayerComponent> player, bool frozen)
    {
        var delta = _lookDelta;
        _lookDelta = Vector2.Zero;

        if (!frozen && MathF.Abs(delta.X) <= MaxLookJump && MathF.Abs(delta.Y) <= MaxLookJump)
        {
            _yaw = WrapAngle(_yaw - delta.X * _sensitivity);
            _pitch = Math.Clamp(_pitch - delta.Y * _sensitivity, -MaxPitch, MaxPitch);
        }

        player.Comp.Yaw = new Angle(_yaw);

        if (frozen
            || _timing.RealTime < _nextLookSend
            || MathF.Abs(_yaw - _sentYaw) < 0.0005f && MathF.Abs(_pitch - _sentPitch) < 0.005f)
        {
            return;
        }

        _sentYaw = _yaw;
        _sentPitch = _pitch;
        _nextLookSend = _timing.RealTime + LookSendInterval;
        RaiseNetworkEvent(new NullspaceLookEvent(_yaw, _pitch));
    }

    private static float WrapAngle(float angle)
    {
        return MathF.Atan2(MathF.Sin(angle), MathF.Cos(angle));
    }
}
